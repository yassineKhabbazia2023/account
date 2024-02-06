// <copyright file="DelegationRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Azure.Core;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Exceptions;
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

    public async Task<int> CreateDelegationAsync(CreateDelegation delegation)
    {
        int result = -1;
        if (delegation is null)
        {
            return result;
        }

        var tDelegation = new TDelegation
        {
            CreationDate = DateTime.UtcNow,
            StartDate = delegation.StartDate,
            EndDate = delegation.EndDate,
            Status = (int)DelegationStatus.PENDING,
            Note = delegation.Note,
        };

        tDelegation.Account = await GetAccountAsync(delegation.GlobalAccountId);
        tDelegation.Delegator = await GetContactAsync(delegation.GlobalDelegatorId);
        tDelegation.Delegatee = await GetContactAsync(delegation.GlobalDelegateeId);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.TDelegation.AddAsync(tDelegation);
            result = await _accountContext.SaveChangesAsync();
        }).ConfigureAwait(false);

        return result;
    }

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(Guid delegateeId)
    {
        var delegationList = new List<TDelegation>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationList = await _accountContext
                                        .TDelegation
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.Delegatee.ContactGlobalUniqueId == delegateeId)
                                        .ToListAsync();
        }).ConfigureAwait(false);

        return delegationList.ToDelegationList();
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegatorId, Guid delegateeId)
    {
        var delegationList = new List<TDelegation>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationList = await _accountContext
                                        .TDelegation
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.Delegator.ContactGlobalUniqueId == delegatorId
                                         &&
                                         d.Delegatee.ContactGlobalUniqueId == delegateeId)
                                        .ToListAsync();
        }).ConfigureAwait(false);

        return delegationList.ToDelegationList();
    }

    private async Task<TContact> GetContactAsync(Guid globlaContactId)
    {
        var tContact = new TContact();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            tContact = await _accountContext
                                        .TContact
                                        .FirstOrDefaultAsync(d => d.ContactGlobalUniqueId == globlaContactId);
        }).ConfigureAwait(false);

        if (tContact is null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, globlaContactId));
        }

        return tContact;
    }

    private async Task<TAccount> GetAccountAsync(Guid globalAccountId)
    {
        var tAccount = new TAccount();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            tAccount = await _accountContext
                                        .TAccount
                                        .FirstOrDefaultAsync(d => d.AccountGlobalUniqueId == globalAccountId);
        }).ConfigureAwait(false);

        if(tAccount is null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
        }

        return tAccount;
    }
}
