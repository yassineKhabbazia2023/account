// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountContext _accountContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public AccountRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;

            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
        }

        public async Task<Paging<AccountModel>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<AccountEntity> query = GetAccountQueryByContactId(criteria.ContactId);

                if (criteria.DeploymentStatus != null)
                {
                    query = from n in query
                            where n.DeploymentEntity.Any(dp => dp.Status.Equals(criteria.DeploymentStatus))
                            select n;
                }

                if (!string.IsNullOrWhiteSpace(criteria.Search))
                {
                    criteria.Search = criteria.Search.ToLowerInvariant();
                    query = from n in query
                            where n.LegalName.ToLower().Contains(criteria.Search)
                                  || n.AccountNumber.ToLower().Contains(criteria.Search)
                                  || n.RoleEntity.Any(role => role.IsSignatory == true && (role.Contact.FirstName.ToLower().Contains(criteria.Search)
                                                      || role.Contact.LastName.ToLower().Contains(criteria.Search)
                                                      || role.Contact.Email.ToLower().Contains(criteria.Search)))
                            select n;
                }

                var totalItems = await query.CountAsync();
                var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

                query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
                query = query.Take(pagination.PageSize);

                return MapAccountDbToAccountModel.MapToPaginAccounts(
                    await query.ToListAsync(),
                    criteria.ContactId,
                    pagination.PageNumber,
                    totalItems,
                    totalPages);
            });
        }

        public async Task<AccountDetail?> GetAccountAsync(int accountId)
        {
            AccountEntity? account = null;
            await _retryPolicy.ExecuteAsync(async () =>
            {
                account = await _accountContext.AccountEntity
                       .AsNoTracking()
                       .FirstOrDefaultAsync(a => a.AccountId == accountId);
            });

            if (account == null)
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
            }

            return account.MapToAccountDetail();
        }

        public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
        {
            AccountEntity? account = null;
            await _retryPolicy.ExecuteAsync(async () =>
            {
                account = await _accountContext.AccountEntity
                       .AsNoTracking()
                       .Include(x => x.RoleEntity)
                       .ThenInclude(r => r.Contact)
                       .Include(a => a.AddressEntity)
                       .Include(x => x.DeploymentEntity)
                       .Include(x => x.Hub)
                       .Include(x => x.Naf)
                       .Include(x => x.PhoneEntity)
                       .FirstOrDefaultAsync(a => a.AccountId == accountId);
            });

            if (account == null)
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
            }

            return account.MapToAccountDetail();
        }

        public async Task UpdateAccountAsync(int accountId, AccountDetail accountDetail)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingAccount = await _accountContext.AccountEntity.SingleAsync(x => x.AccountId == accountId);
                existingAccount.MapToUpdatedAccount(accountDetail);
                _accountContext.AccountEntity.Update(existingAccount);
                await _accountContext.SaveChangesAsync();
            });
        }

        public async Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination pagination)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<ContactEntity> query = GetContactEntitiesByAccountId(accountId);

                if (!string.IsNullOrWhiteSpace(criteria.Search))
                {
                    criteria.Search = criteria.Search.ToLowerInvariant();
                    query = from n in query
                            where n.Email.ToLower().Contains(criteria.Search)
                                  || n.FirstName.ToLower().Contains(criteria.Search)
                                  || n.LastName.ToLower().Contains(criteria.Search)
                                  || n.PersonaName.ToLower().Contains(criteria.Search)
                                  || (n.Office != null && n.Office.ToLower().Contains(criteria.Search))
                            select n;
                }

                if (criteria.Type != null && System.Enum.IsDefined(typeof(ContactType), criteria.Type))
                {
                    query = query.Where(x => x.Type.Equals(criteria.Type.ToString()));
                }

                var totalItems = await query.CountAsync();

                var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

                query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
                query = query.Take(pagination.PageSize == 0 ? totalItems : pagination.PageSize);

                var result = await query.ToListAsync();
                if(result == null)
                {
                    throw new NotFoundException(Errors.NotFoundContactsCode, Errors.NotFoundContactsMessage);
                }

                var contacts = result.Select(c => c.MapToContact());

                return contacts!.MapToPagingContact(
                    pagination.PageNumber,
                    totalItems,
                    totalPages);
            });
        }

        public async Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination pagination)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<int> accountIds = GetAccountQueryByContactId(contactId).Select(account => account.AccountId);
                if (accountIds?.Any() != true)
                {
                    throw new NotFoundException(Errors.NotFoundRoleContactCode, string.Format(Errors.NotFoundRoleContactMessage, contactId));
                }

                var result = await _accountContext.RoleEntity
                        .AsNoTracking()
                        .Include(x => x.Contact)
                        .Where(x => accountIds.Contains(x.AccountId) && x.Contact.Type == request.ContactType.ToString().ToLower())
                        .ToListAsync();

                var resultContact = result.DistinctBy(x => x.ContactId);

                if (!request.Search.IsNullOrEmpty())
                {
                    resultContact = resultContact.Where(role => role.Contact.Email.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                                                                || role.Contact.FirstName.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                                                                || role.Contact.LastName.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                                                                || role.Contact.PersonaName.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)
                                                                || (role.Contact.Office is not null && role.Contact.Office.Contains(request.Search!, StringComparison.OrdinalIgnoreCase)));
                }

                var totalItems = resultContact.Count();
                var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

                resultContact = resultContact.Skip((pagination.PageNumber - 1) * pagination.PageSize);
                resultContact = resultContact.Take(pagination.PageSize);

                return resultContact
                        .ToList()
                        .MapToContacts()
                        .MapToPagingContact(pagination.PageNumber, totalItems, totalPages);
            });
        }

        private IQueryable<AccountEntity> GetAccountQueryByContactId(int contactId)
        {
            return _accountContext.AccountEntity
                            .AsNoTracking()
                            .Include(x => x.RoleEntity)
                            .ThenInclude(r => r.Contact)
                            .Include(a => a.AddressEntity)
                            .Include(x => x.DeploymentEntity)
                            .Where(a => a.RoleEntity.Any(r => r.ContactId == contactId))
                            .OrderBy(a => a.LegalName);
        }

        private IQueryable<ContactEntity> GetContactEntitiesByAccountId(int accountId)
        {
            return _accountContext.RoleEntity.AsNoTracking()
                        .Include(x => x.Contact)
                        .Where(x => x.AccountId == accountId)
                        .Select(x => x.Contact);
        }
    }
}
