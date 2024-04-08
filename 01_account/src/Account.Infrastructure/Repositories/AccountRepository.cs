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
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;
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

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<AccountEntity> query = GetAccountQueryByContactId(contactId);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = from n in query
                            where n.LegalName.Contains(search, StringComparison.OrdinalIgnoreCase)
                                  || n.AccountNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                                  || n.RoleEntity.Any(role => role.IsSignatory == true && (role.Contact.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                                                      || role.Contact.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                                                      || role.Contact.Email.Contains(search, StringComparison.OrdinalIgnoreCase)))
                            select n;
                }

                var totalItems = await query.CountAsync();
                var totalPages = Pagination.GetTotalPages(totalItems, pageSize);

                query = query.Skip((pageNumber - 1) * pageSize);
                query = query.Take(pageSize);

                return MapAccountDbToAccountModel.MapToPaginAccounts(
                    await query.ToListAsync(),
                    contactId,
                    pageNumber,
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

        public async Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId, ContactType? type)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<RoleEntity> query =
                    _accountContext.RoleEntity
                        .AsNoTracking()
                        .Include(x => x.Contact)
                        .Where(x => x.AccountId == accountId);

                if (type != null && System.Enum.IsDefined(typeof(ContactType), type))
                {
                    query = query.Where(x => x.Contact.Type.Equals(type.ToString()));
                }

                var result = await query.ToListAsync();

                return result.MapToContacts();
            });
        }

        public async Task<Paging<Contact>> GetContactsAccountByAdminAsync(string? search, int contactId, int pageNumber, int pageSize)
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
                        .Where(x => accountIds.Contains(x.AccountId))
                        .ToListAsync();

                var resultContact = result.DistinctBy(x => x.ContactId);

                if (!search.IsNullOrEmpty())
                {
                    resultContact = resultContact.Where(role => role.Contact.Email.Contains(search!, StringComparison.OrdinalIgnoreCase)
                                                                || role.Contact.FirstName.Contains(search!, StringComparison.OrdinalIgnoreCase)
                                                                || role.Contact.LastName.Contains(search!, StringComparison.OrdinalIgnoreCase)
                                                                || role.Contact.PersonaName.Contains(search!, StringComparison.OrdinalIgnoreCase));
                }

                var totalItems = resultContact.Count();
                var totalPages = Pagination.GetTotalPages(totalItems, pageSize);

                resultContact = resultContact.Skip((pageNumber - 1) * pageSize);
                resultContact = resultContact.Take(pageSize);

                return resultContact
                        .ToList()
                        .MapToContacts()
                        .MapToPagingContact(pageNumber, totalItems, totalPages);
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
    }
}
