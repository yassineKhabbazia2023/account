// <copyright file="AccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
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

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit, int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<TAccount> entities = GetAccountQueryByContactId(contactId);

                if (!search.IsNullOrEmpty())
                {
                    entities = from n in entities
                               where n.LegalName.Contains(search)
                                     || n.SourceAccountNumber.Contains(search)
                                     || n.TRole.Any(role => role.IsSignatory == true && (role.Contact.FirstName.Contains(search)
                                                         || role.Contact.LastName.Contains(search)
                                                         || role.Contact.ContactEmail.Contains(search)))
                               select n;
                }

                var count = await entities.CountAsync();

                entities = entities.Skip((page - 1) * limit);
                entities = entities.Take(limit);

                var totalPageCalcul = PagesCalculator.GetTotalPages(count, limit);

                var pageinateResult = new Paging<AccountModel>()
                {
                    Items = entities.Select(entity => entity.MapTAccountToAccountModel()),
                    CurrentPage = page,
                    TotalItems = count,
                    TotalPage = (int)Math.Ceiling(totalPageCalcul)
                };
                return pageinateResult;
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
                       .Include(x => x.TPhone)
                       .Where(a => a.AccountId == accountId);

                var entity = await entities.FirstOrDefaultAsync();
                if (entity == null)
                {
                    throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotFoundError);
                }

                var accountDetail = entity.MapTAccountToAccountDetail();

                return accountDetail;
            }).ConfigureAwait(false);
        }

        public async Task<AccountDetail?> UpdateAccountAsync(AccountDetail accountDetail, int accountId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingAccounts = from account in _accountContext.TAccount
                                       where account.AccountId.Equals(accountId)
                                       select account;

                var existingAccount = await existingAccounts.FirstOrDefaultAsync();
                if (existingAccount == null)
                {
                    throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotFoundError);
                }

                if (accountDetail == null)
                {
                    throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotNullException);
                }

                existingAccount.MapUpdatedAccount(accountDetail);
                _accountContext.TAccount.Update(existingAccount);
                await _accountContext.SaveChangesAsync();
                return await GetAccountDetailAsync(accountId);
            }).ConfigureAwait(false);
        }

        public async Task<Statistics> GetStatisticsAsync(int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var entities = _accountContext.TDeploymentPlanning
                .Join(_accountContext.TRole,
                deployment => deployment.AccountId,
                role => role.AccountId,
                (deployment, role) => new { deployment, role })
                .Where(x => x.role.ContactId == contactId)
                .GroupBy(x => x.deployment.Status)
                .Select(s => new { Status = s.Key, Count = s.Select(d => d.deployment.Status).Count() });

                var countByStatus = await entities.ToDictionaryAsync(x => x.Status, x => x.Count);

                return MapAccountDbToAccountModel.MapStatistics(countByStatus);
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<AccountFavorite>> GetAccountsFavoriteAsync(int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var entities = GetAccountQueryByContactId(contactId);
                var accountFavorite = entities
                    .Where(entity => entity.TRole.Any(role => role.IsFavorite == true))
                    .Select(entity => new AccountFavorite()
                    {
                        AccountId = entity.AccountId,
                        LegalName = entity.LegalName,
                        IconName = entity.IconName
                    });

                return accountFavorite;
            }).ConfigureAwait(false);
        }

        public async Task UpdateAccountFavoriteAsync(int accountId, int contactId, bool isFavorite)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingRole = from role in _accountContext.TRole
                                   where role.AccountId.Equals(accountId) && role.ContactId.Equals(contactId)
                                       select role;

                var existingRoleItem = await existingRole.FirstOrDefaultAsync();
                if (existingRoleItem != null)
                {
                    existingRoleItem.IsFavorite = isFavorite;
                    _accountContext.TRole.Update(existingRoleItem);
                    await _accountContext.SaveChangesAsync();
                }
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
