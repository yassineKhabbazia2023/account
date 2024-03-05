// <copyright file="StatisticsRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly.Retry;
using Polly;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Core.Constants;
using Microsoft.EntityFrameworkCore;
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
                var entities = _accountContext.DeploymentEntity
                    .Join(_accountContext.RoleEntity,
                        deployment => deployment.AccountId,
                        role => role.AccountId,
                        (deployment, role) => new { deployment, role })
                    .Where(x => x.role.ContactId == contactId)
                    .GroupBy(x => x.deployment.Status)
                    .Select(s => new { Status = s.Key, Count = s.Select(d => d.deployment.Status).Count() });

                var countByStatus = await entities.ToDictionaryAsync(x => x.Status, x => x.Count);

                return MapAccountDbToAccountModel.MapToStatistics(countByStatus);
            });
        }
    }
}
