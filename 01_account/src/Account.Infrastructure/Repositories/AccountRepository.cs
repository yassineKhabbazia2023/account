// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using System.Net;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Core.Extensions;
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
                IQueryable<TAccount> query = GetAccountQueryByContactId(contactId);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = from n in query
                            where n.LegalName.Contains(search)
                                  || n.SourceAccountNumber.Contains(search)
                                  || n.TRole.Any(role => role.IsSignatory == true && (role.Contact.FirstName.Contains(search)
                                                      || role.Contact.LastName.Contains(search)
                                                      || role.Contact.ContactEmail.Contains(search)))
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
            }).ConfigureAwait(false);
        }

        public async Task<AccountDetail?> GetAccountAsync(int accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var account = await _accountContext.TAccount
                       .AsNoTracking()
                       .FirstOrDefaultAsync(a => a.AccountId == accountId);

                if (account == null)
                {
                    throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
                }

                return account.MapToAccountDetail();
            }).ConfigureAwait(false);
        }

        public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var account = await _accountContext.TAccount
                       .AsNoTracking()
                       .Include(x => x.TRole)
                       .ThenInclude(r => r.Contact)
                       .Include(a => a.TAddress)
                       .Include(x => x.TDeploymentPlanning)
                       .Include(x => x.Hub)
                       .Include(x => x.Naf)
                       .Include(x => x.TPhone)
                       .FirstOrDefaultAsync(a => a.AccountId == accountId);

                if (account == null)
                {
                    throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
                }

                return account.MapToAccountDetail();
            }).ConfigureAwait(false);
        }

        public async Task UpdateAccountAsync(int accountId, AccountDetail accountDetail)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingAccount = await _accountContext.TAccount.SingleAsync(x => x.AccountId == accountId);
                existingAccount.MapToUpdatedAccount(accountDetail);
                _accountContext.TAccount.Update(existingAccount);
                await _accountContext.SaveChangesAsync();
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = await _accountContext.TRole
                    .AsNoTracking()
                    .Include(x => x.Contact)
                    .Where(x => x.AccountId == accountId)
                    .ToListAsync();

                return result.MapToContacts();
            }).ConfigureAwait(false);
        }

        private IQueryable<TAccount> GetAccountQueryByContactId(int contactId)
        {
            return _accountContext.TAccount
                            .AsNoTracking()
                            .Include(x => x.TRole)
                            .ThenInclude(r => r.Contact)
                            .Include(a => a.TAddress)
                            .Include(x => x.TDeploymentPlanning)
                            .Where(a => a.TRole.Any(r => r.ContactId == contactId))
                            .OrderBy(a => a.LegalName);
        }
    }
}
