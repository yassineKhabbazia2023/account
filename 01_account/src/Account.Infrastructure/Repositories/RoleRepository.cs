// <copyright file="RoleRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Utils;

namespace Pulse.Account.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RoleRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
    }

    public async Task<Paging<Core.Models.Account>> GetContactRolesAsync(int contactId, int page, int limit)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IEnumerable<TAccount> entities = await _accountContext.TAccount
                                                        .AsNoTracking()
                                                        .Include(x => x.TRoles)
                                                        .ThenInclude(r => r.Contact)
                                                        .Include(a => a.TAddress)
                                                        .Include(x => x.TDeploymentPlanning)
                                                        .Where(a => a.TRoles.Any(r => r.ContactId == contactId))
                                                        .OrderBy(x => x.LegalName).ToListAsync();

            var count = entities.Count();

            entities = entities.Skip((page - 1) * limit);
            entities = entities.Take(limit);

            var totalPageCalcul = AccountUtils.CalculTotalPage(count, limit);

            var pageinateResult = new Paging<Core.Models.Account>()
            {
                Items = entities.Select(entity => entity.MapTAccountToAccountModel()),
                CurrentPage = page,
                TotalItems = count,
                TotalPage = (int)Math.Ceiling(totalPageCalcul)
            };

            return pageinateResult;
        }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Signatory>> GetSignatoryAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IReadOnlyCollection<TRoles> res = await _accountContext.TRoles
                .AsNoTracking()
            .Include(x => x.Contact)
                .Where(x => x.AccountId == accountId && x.IsSignatory!.Value).ToListAsync();

            return res.MapTRolesToSignatory();
        }).ConfigureAwait(false);
    }
}
