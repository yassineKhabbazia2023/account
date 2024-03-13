// <copyright file="StatisticsRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Enum;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly AccountContext _accountContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public StatisticsRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;

            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
        }

        public async Task<Statistics> GetStatisticsAsync(int contactId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var accountStats = await GetAccountStatistics(contactId);
                var contactStats = await GetContactStatistics(contactId);

                return MapAccountDbToAccountModel.MapToStatistics(accountStats!, contactStats!);
            });
        }

        private async Task<Dictionary<int, int>?> GetAccountStatistics(int contactId)
        {
            var entities = _accountContext.DeploymentEntity
                    .Join(_accountContext.RoleEntity,
                        deployment => deployment.AccountId,
                        role => role.AccountId,
                        (deployment, role) => new { deployment, role })
                    .Where(x => x.role.ContactId == contactId)
                    .GroupBy(x => x.deployment.Status)
                    .Select(s => new { Status = s.Key, Count = s.Select(d => d.deployment.Status).Count() });

            return await entities.ToDictionaryAsync(x => x
            .Status, x => x.Count);
        }

        private async Task<Dictionary<string, int>?> GetContactStatistics(int contactId)
        {
            var accountIds = _accountContext.RoleEntity
                .Where(r => r.ContactId == contactId)
                .Select(r => r.AccountId);

            var contactIds = accountIds
                .Join(_accountContext.RoleEntity,
                    ids => ids,
                    role => role.AccountId,
                    (ids, role) => role)
                .Select(r => r.ContactId).Distinct();

            var contactStatusStatistics = contactIds
                .Join(_accountContext.ContactEntity,
                    ids => ids,
                    contact => contact.ContactId,
                    (role, contact) => contact)
                .Where(contact => contact.Type == ContactType.Client.ToString())
                .GroupBy(x => x.Status)
                .Select(s => new { Status = s.Key, Count = s.Select(d => d.Status).Count() });

            return await contactStatusStatistics.ToDictionaryAsync(x => x.Status, x => x.Count);
        }
    }
}
