// <copyright file="FavoriteRepository.cs" company="Pulse">
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
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
        }

        public async Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var accountFavorite = await _accountContext.RoleEntity
                    .Include(role => role.Account)
                    .Where(role => role.ContactId == contactId && role.IsFavorite == true)
                    .Select(entity => new AccountFavorite()
                    {
                        AccountId = entity.AccountId,
                        LegalName = entity.Account.LegalName,
                        IconName = entity.Account.IconName
                    }).ToListAsync();

                return accountFavorite;
            });
        }

        public async Task UpdateAccountFavoriteAsync(int accountId, int contactId, bool isFavorite)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var existingRole = from role in _accountContext.RoleEntity
                                   where role.AccountId.Equals(accountId) && role.ContactId.Equals(contactId)
                                       select role;

                var existingRoleItem = await existingRole.FirstOrDefaultAsync();
                if (existingRoleItem == null)
                {
                    throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
                }

                existingRoleItem.IsFavorite = isFavorite;
                _accountContext.RoleEntity.Update(existingRoleItem);
                await _accountContext.SaveChangesAsync();
            });
        }
    }
}
