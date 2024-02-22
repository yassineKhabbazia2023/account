// <copyright file="ReferentialRepository.cs" company="Pulse">
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
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class ReferentialRepository : IReferentialRepository
    {
        private readonly AccountContext _accountContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ReferentialRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;
            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
        }

        public async Task<IEnumerable<Hub?>> GetHubsAsync()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var hubs = await _accountContext.THub.AsNoTracking().ToListAsync();

                return hubs.MapHubEntitiesToHubs();
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Naf?>> GetNafsAsync()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var nafs = await _accountContext.TNaf.AsNoTracking().ToListAsync();

                return nafs.MapNafEntitiesToNafs();
            }).ConfigureAwait(false);
        }
    }
}
