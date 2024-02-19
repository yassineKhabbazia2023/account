// <copyright file="FavoriteRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AccountContext _accountContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public FavoriteRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;

            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
        }

        public async Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId)
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
