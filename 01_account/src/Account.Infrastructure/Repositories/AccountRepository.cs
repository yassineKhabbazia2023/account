// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using System.Net;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
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

                var totalRows = await query.CountAsync();

                query = query.Skip((pageNumber - 1) * pageSize);
                query = query.Take(pageSize);

                var totalPages = PagesCalculator.GetTotalPages(totalRows, pageSize);

                return MapAccountDbToAccountModel.MapToPaginAccounts(
                    await query.ToListAsync(),
                    contactId,
                    pageNumber,
                    totalRows,
                    totalPages);
            }).ConfigureAwait(false);
        }

        public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<TAccount> entities = _accountContext.TAccount
                       .AsNoTracking()
                       .Include(x => x.TRole)
                       .ThenInclude(r => r.Contact)
                       .Include(a => a.TAddress)
                       .Include(x => x.TDeploymentPlanning)
                       .Include(x => x.Hub)
                       .Include(x => x.Naf)
                       .Include(x => x.TPhone)
                       .Where(a => a.AccountId == accountId);

                var entity = await entities.FirstOrDefaultAsync() ?? throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotFoundError);

                return entity.MapToAccountDetail();
            }).ConfigureAwait(false);
        }

        public async Task<AccountDetail?> UpdateAccountAsync(AccountDetail accountDetail, int accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingAccounts = from account in _accountContext.TAccount
                                       where account.AccountId.Equals(accountId)
                                       select account;

                var existingAccount = await existingAccounts.FirstOrDefaultAsync() ?? throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotFoundError);

                existingAccount.MapToUpdatedAccount(accountDetail);
                _accountContext.TAccount.Update(existingAccount);
                await _accountContext.SaveChangesAsync();
                return await GetAccountDetailAsync(accountId);
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
