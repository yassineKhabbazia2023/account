// <copyright file="DelegationRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories;

public class DelegationRepository : IDelegationRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public DelegationRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _retryPolicy = Policy.Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(3000));
    }

    public async Task<Guid> CreateDelegationAsync(CreateDelegation delegation)
    {
        Guid result = Guid.Empty;
        if (delegation is null)
        {
            return result;
        }

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        }).ConfigureAwait(false);

        return result;
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegateeId)
    {
        var delegationList = new List<TDelegation>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationList = await _accountContext
                                        .TDelegation
                                        .Where(d => d.Delegatee.ContactGlobalUniqueId == delegateeId)
                                        .ToListAsync();
        }).ConfigureAwait(false);

        return delegationList.ToDelegationList();
    }

    public Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegatorId, Guid delegateeId)
        => throw new NotImplementedException();
}
