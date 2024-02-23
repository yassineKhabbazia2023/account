// <copyright file="ReferentialRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
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

        public async Task<Paging<Naf>> GetNafsAsync(string? search, int pageNumber, int pageSize)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<TNaf?> query = GetNafsWithCriteria(search);

                var totalRows = await query.CountAsync();

                query = query.Skip((pageNumber - 1) * pageSize);
                query = query.Take(pageSize);

                var totalPages = PagesCalculator.GetTotalPages(totalRows, pageSize);

                var nafs = await query.ToListAsync();

                return nafs.MapToPagingNaf(pageNumber, totalRows, totalPages);
            }).ConfigureAwait(false);
        }

        private IQueryable<TNaf?> GetNafsWithCriteria(string search)
        {
            return _accountContext.TNaf.AsNoTracking()
                .Where(n => n.NafCode.Contains(search ?? string.Empty));
        }
    }
}
