// <copyright file="ReferentialRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
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
                var hubs = await _accountContext.HubEntity.AsNoTracking().ToListAsync();

                return hubs.MapHubEntitiesToHubs();
            });
        }

        public async Task<Paging<Naf>> GetNafsAsync(string? search, int pageNumber, int pageSize)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                IQueryable<NafEntity?> query = GetNafsWithCriteria(search);

                var totalRows = await query.CountAsync();

                query = query.Skip((pageNumber - 1) * pageSize);
                query = query.Take(pageSize);

                var totalPages = Pagination.GetTotalPages(totalRows, pageSize);

                var nafs = await query.ToListAsync();

                return nafs.MapToPagingNaf(pageNumber, totalRows, totalPages);
            });
        }

        private IQueryable<NafEntity?> GetNafsWithCriteria(string search)
        {
            return _accountContext.NafEntity.AsNoTracking()
                .Where(n => n.NafCode.Contains(search ?? string.Empty));
        }
    }
}
