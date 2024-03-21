// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
        }

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<AccountEntity> query = GetAccountQueryByContactId(contactId);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = from n in query
                            where n.LegalName.Contains(search)
                                  || n.SourceAccountNumber.Contains(search)
                                  || n.RoleEntity.Any(role => role.IsSignatory == true && (role.Contact.FirstName.Contains(search)
                                                      || role.Contact.LastName.Contains(search)
                                                      || role.Contact.Email.Contains(search)))
                            select n;
                }

                query = query.Skip((pageNumber - 1) * pageSize);
                query = query.Take(pageSize);

                var totalItems = await query.CountAsync();
                var totalPages = Pagination.GetTotalPages(totalItems, pageSize);

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

        public async Task<IEnumerable<Contact>> GetContactsAccountByAdminAsync(int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<int> accountIds = GetAccountQueryByContactId(contactId).Select(account => account.AccountId) ?? throw new NotFoundException(Errors.NotFoundRoleContactCode, string.Format(Errors.NotFoundRoleContactMessage, contactId));

                IQueryable<RoleEntity> query = _accountContext.RoleEntity
                        .AsNoTracking()
                        .Include(x => x.Contact)
                        .Where(x => accountIds.Contains(x.AccountId));

                var result = await query.ToListAsync();

                return result.MapToContacts().DistinctBy(x => x.ContactId);
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
