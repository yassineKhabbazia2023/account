// <copyright file="DelegationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
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

    public async Task<int> CreateDelegationAsync(CreateDelegationRequest delegation)
    {
        int result = -1;
        if (delegation is null)
        {
            throw new BadRequestException(Errors.CreateDelegationCode, Errors.CreateDelegationMessage);
        }

        var tDelegation = new TDelegation
        {
            CreationDate = DateTime.UtcNow,
            StartDate = delegation.StartDate!.Value,
            EndDate = delegation.EndDate,
            Status = delegation.Status,
            Note = delegation.Note,
        };

        tDelegation.Account = await GetAccountAsync(delegation.AccountId);
        tDelegation.Delegator = await GetContactAsync(delegation.DelegatorId);
        tDelegation.Delegatee = await GetContactAsync(delegation.DelegateeId);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.TDelegation.AddAsync(tDelegation);
            result = await _accountContext.SaveChangesAsync();
        });

        return result;
    }

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId)
    {
        var delegationList = new List<TDelegation>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationList = await _accountContext
                                        .TDelegation
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.DelegateeId == delegateeId)
                                        .ToListAsync();
        });

        return delegationList.ToDelegations();
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId)
    {
        var delegations = new List<TDelegation>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegations = await _accountContext
                                        .TDelegation
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.DelegatorId == delegatorId
                                         &&
                                         d.DelegateeId == delegateeId)
                                        .ToListAsync();
        });

        return delegations.ToDelegations();
    }

    public async Task DeleteDelegationAsync(int delegationId)
    {
        if (delegationId <= 0)
        {
            throw new BadRequestException(Errors.BadRequestDeleteDelegationCode, Errors.BadRequestDeleteDelegationMessage);
        }

        var tDelegation = await GetDelegationAsync(delegationId);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            tDelegation.Status = Constants.DISABLEDDELEGATIONSTATUS;
            _accountContext.TDelegation.Update(tDelegation);
            await _accountContext.SaveChangesAsync();
        });
    }

    private async Task<TContact> GetContactAsync(int contactId)
    {
        var tContact = new TContact();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            tContact = await _accountContext
                                        .TContact
                                        .FirstOrDefaultAsync(d => d.ContactId == contactId);
        });

        if (tContact is null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        return tContact;
    }

    private async Task<TAccount> GetAccountAsync(int accountId)
    {
        var tAccount = new TAccount();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            tAccount = await _accountContext
                                        .TAccount
                                        .FirstOrDefaultAsync(d => d.AccountId == accountId);
        });

        if(tAccount is null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return tAccount;
    }

    private async Task<TDelegation> GetDelegationAsync(int delegationId)
    {
        TDelegation? tDelegation = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            tDelegation = await _accountContext.TDelegation.FirstOrDefaultAsync(d => d.DelegationId == delegationId);
        });

        if (tDelegation == null)
        {
            throw new NotFoundException(Errors.NotFoundDelegationCode, string.Format(Errors.NotFoundDelegationMessage, delegationId));
        }

        return tDelegation;
    }
}
