// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using System.Linq.Expressions;
using Kpmg.ExceptionMiddleware.AdvancedException;
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

        public async Task<AccountDetail> UpdateAccountAsync(int accountId, AccountDetail accountDetail)
        {
            AccountDetail? toReturn = null!;
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingAccount = await _accountContext.AccountEntity.SingleAsync(x => x.AccountId == accountId);
                if (existingAccount == null)
                {
                    throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
                }

                existingAccount.MapToUpdatedAccount(accountDetail);
                _accountContext.AccountEntity.Update(existingAccount);
                toReturn = existingAccount.MapToAccountDetail();
                await _accountContext.SaveChangesAsync();
            });

            return toReturn!;
        }

        public async Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination pagination)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<ContactEntity> query = GetContactEntitiesByAccountId(accountId);

                if (!string.IsNullOrWhiteSpace(criteria.Search))
                {
                    var search = criteria.Search.ToLowerInvariant();
                    query = from n in query
                            where n.Email.ToLower().Contains(search)
                                  || n.FirstName.ToLower().Contains(search)
                                  || n.LastName.ToLower().Contains(search)
                                  || n.PersonaName.ToLower().Contains(search)
                                  || ((n.Office != null && n.Office.ToLower().Contains(search))
                                  || (!string.IsNullOrWhiteSpace(n.Status) && n.Status.ToLower().Contains(search)))
                            select n;
                }

                if (criteria.Type != null && System.Enum.IsDefined(typeof(ContactType), criteria.Type))
                {
                    query = query.Where(x => x.Type.Equals(criteria.Type.ToString()));
                }

                query = GetContactEntitiesSorted(query, criteria.Sorting);

                var totalItems = await query.CountAsync();

                var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

                query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
                query = query.Take(pagination.PageSize);

                var result = await query.ToListAsync();
                if (result == null)
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

                var query = _accountContext.RoleEntity
                        .AsNoTracking()
                        .Include(x => x.Contact)
                        .Where(x => accountIds.Contains(x.AccountId) && x.Contact.Type == request.ContactType.ToString().ToLower());

                if (!request.Search.IsNullOrEmpty())
                {
                    query = query.Where(role => role.Contact.Email.ToLower().Contains(request.Search!.ToLower())
                                                                || role.Contact.FirstName.ToLower().Contains(request.Search.ToLower())
                                                                || role.Contact.LastName.ToLower().Contains(request.Search.ToLower())
                                                                || role.Contact.PersonaName.ToLower().Contains(request.Search.ToLower())
                                                                || (role.Contact.Office != null && role.Contact.Office.ToLower().Contains(request.Search.ToLower())));
                }

                query = query.GroupBy(x => x.ContactId).Select(g => g.First());
                var totalItems = await query.CountAsync();
                var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

                query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
                query = query.Take(pagination.PageSize);

                return (await query
                        .ToListAsync())
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
                            .Include(x => x.Hub)
                            .Where(a => a.RoleEntity.Any(r => r.ContactId == contactId) && a.DeploymentEntity.First().Status != (int)DeploymentStatus.Revoked)
                            .OrderBy(a => a.LegalName);
        }

        private IQueryable<ContactEntity> GetContactEntitiesByAccountId(int accountId)
        {
            return _accountContext.RoleEntity.AsNoTracking()
                        .Include(x => x.Contact)
                        .Where(x => x.AccountId == accountId)
                        .Select(x => x.Contact);
        }

        private static IQueryable<ContactEntity> GetContactEntitiesSorted(IQueryable<ContactEntity> query, Sorting? sorting)
        {
            if (sorting != null)
            {
                Expression<Func<ContactEntity, object>> exp = null!;
                switch (sorting.Field)
                {
                    case SortingConstants.NAME:
                        exp = c => c.FirstName + c.LastName;
                        break;
                    case SortingConstants.EMAIL:
                        exp = c => c.Email;
                        break;
                    case SortingConstants.PERSONA:
                        exp = c => c.PersonaName;
                        break;
                    case SortingConstants.OFFICE:
                        exp = c => c.Office;
                        break;
                    case SortingConstants.STATUS:
                        exp = c => c.Status;
                        break;
                    case SortingConstants.DATE:
                        exp = c => c.CreationDate;
                        break;
                    default:
                        throw new BadRequestException(Errors.BadRequestContactsAccountCode, string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field));
                }

                if (sorting.Descending)
                {
                    query = query.OrderByDescending(exp);
                }
                else
                {
                    query = query.OrderBy(exp);
                }
            }

            return query;
        }
    }
}
