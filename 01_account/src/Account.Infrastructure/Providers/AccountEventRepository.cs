// <copyright file="AccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly.Retry;
using Polly;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Extensions;

namespace Pulse.Account.Infrastructure.Providers;

public class AccountEventRepository : IAccountEventRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AccountEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<IEnumerable<int>> UpdateAccountStatusByContactAsync(IEnumerable<int> accountIds, int deploymentStatus)
    {
        var accountDeployment = _accountContext.DeploymentEntity.Where(x => accountIds.Contains(x.AccountId));
        foreach (var item in accountDeployment)
        {
            item.Status = deploymentStatus;
        }

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });

        return accountDeployment.Select(x => x.AccountId);
    }

    public List<AccountEntity> GetAccountBySignatory(int contactId)
    {
        return _accountContext.AccountEntity
                    .AsNoTracking()
                    .Include(x => x.RoleEntity)
                    .ThenInclude(r => r.Contact)
                    .Where(a => a.RoleEntity.Any(r => r.ContactId == contactId && r.IsSignatory == true))
                    .ToList();
    }
}
