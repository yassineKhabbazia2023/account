// <copyright file="DelegationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
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
        var delegationEntities = new List<DelegationEntity>();
        var delegator = await GetContactAsync(delegation.DelegatorId);

        foreach (var detail in delegation.DelegationDetails)
        {
            var delegationEntity = new DelegationEntity
            {
                CreationDate = DateTime.UtcNow,
                Delegator = delegator,
                Delegatee = await GetContactAsync(detail.DelegateeId),
                StartDate = detail.StartDate!.Value,
                EndDate = detail.EndDate,
                Status = detail.Status,
                Note = detail.Note,
                Account = new List<AccountEntity>()
            };

            foreach (var accountId in delegation.AccountIds)
            {
                delegationEntity.Account.Add(await GetAccountAsync(accountId));
            }

            delegationEntities.Add(delegationEntity);
        }

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.DelegationEntity.AddRangeAsync(delegationEntities);
            await _accountContext.SaveChangesAsync();
        });

        return StatusCodes.Status201Created;
    }

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId)
    {
        var delegationList = new List<DelegationEntity>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationList = await _accountContext.DelegationEntity
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
        var delegations = new List<DelegationEntity>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegations = await _accountContext.DelegationEntity
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.DelegatorId == delegatorId && d.DelegateeId == delegateeId)
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
            tDelegation.Status = DelegationStatus.Disabled.ToString().ToLower();
            _accountContext.DelegationEntity.Update(tDelegation);
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task<IReadOnlyCollection<Delegation>> GetAccountDelegationsHistoryAsync(int accountId)
    {
        var delegations = new List<DelegationEntity>();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegations = await _accountContext.DelegationEntity
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.Account.Any(a => a.AccountId == accountId))
                                        .ToListAsync();
        });

        return delegations.ToDelegations();
    }

    public async Task<bool> DoesAccountExistAsync(int accountId)
    {
        var validAccount = false;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            validAccount = await _accountContext
                                        .AccountEntity
                                        .AnyAsync(d => d.AccountId == accountId);
        });

        return validAccount;
    }

    private async Task<ContactEntity> GetContactAsync(int contactId)
    {
        var tContact = new ContactEntity();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            tContact = await _accountContext
                                        .ContactEntity
                                        .FirstOrDefaultAsync(d => d.ContactId == contactId);
        });

        if (tContact is null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        return tContact;
    }

    private async Task<AccountEntity> GetAccountAsync(int accountId)
    {
        var tAccount = new AccountEntity();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            tAccount = await _accountContext
                                        .AccountEntity
                                        .FirstOrDefaultAsync(d => d.AccountId == accountId);
        });

        if (tAccount is null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return tAccount;
    }

    private async Task<DelegationEntity> GetDelegationAsync(int delegationId)
    {
        DelegationEntity? tDelegation = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            tDelegation = await _accountContext.DelegationEntity.FirstOrDefaultAsync(d => d.DelegationId == delegationId);
        });

        if (tDelegation == null)
        {
            throw new NotFoundException(Errors.NotFoundDelegationCode, string.Format(Errors.NotFoundDelegationMessage, delegationId));
        }

        return tDelegation;
    }
}
