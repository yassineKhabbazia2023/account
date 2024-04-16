// <copyright file="DelegationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
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

    public async Task CreateDelegationAsync(CreateDelegationRequest delegation, IEnumerable<CreateRoleRequest> roles)
    {
        ArgumentNullException.ThrowIfNull(delegation);
        var contactsToCheck = delegation.DelegationDetails.Select(d => d.DelegateeId).ToList();
        contactsToCheck.Add(delegation.DelegatorId);

        if (!(await CheckExistingContactsAsync(contactsToCheck))?.Any() == false)
        {
            throw new NotFoundException(Errors.NotFoundContactsCode, Errors.NotFoundContactsMessage);
        }

        if (!(await CheckExistingAccountsAsync(delegation.AccountIds))?.Any() == false)
        {
            throw new NotFoundException(Errors.NotFoundAccountsCode, Errors.NotFoundAccountsMessage);
        }

        var accounts = await GetAccountsAsync(delegation.AccountIds);
        var delegationEntities = delegation.MapDelegationRequestToDelegationsDb(accounts.ToList());
        var roleEntities = roles.MapRolesToRoleDb();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            if (roles.Any())
            {
                await _accountContext.RoleEntity.AddRangeAsync(roleEntities);
            }

            await _accountContext.DelegationEntity.AddRangeAsync(delegationEntities);
            await _accountContext.SaveChangesAsync();
        });
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

    public async Task<IEnumerable<Role>> DeleteDelegationAsync(int delegationId)
    {
        var delegationEntity = await GetDelegationAsync(delegationId);
        var roles = Enumerable.Empty<Role>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationEntity.Status = DelegationStatus.Disabled.ToString().ToLower();
            _accountContext.DelegationEntity.Update(delegationEntity);

            if (delegationEntity.StartDate.Date <= DateTime.UtcNow.Date)
            {
                var roleEntities = await GetRoleForDelegationAsync(delegationEntity.Account.Select(a => a.AccountId), delegationEntity.DelegateeId);

                if (roleEntities.Any())
                {
                    roles = roleEntities.MapToRoles();
                    _accountContext.RoleEntity.RemoveRange(roleEntities);
                }
            }

            await _accountContext.SaveChangesAsync();
        });

        return roles;
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
            validAccount = await _accountContext.AccountEntity.AnyAsync(d => d.AccountId == accountId);
        });

        return validAccount;
    }

    private async Task<IEnumerable<int>> CheckExistingContactsAsync(IEnumerable<int> contactIds)
    {
        var missingIds = new List<int>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var dbIds = await _accountContext.ContactEntity.Select(c => c.ContactId).ToListAsync();
            missingIds = contactIds.Except(dbIds).ToList();
        });

        return missingIds;
    }

    private async Task<IEnumerable<AccountEntity>> GetAccountsAsync(IEnumerable<int> accountIds)
    {
        var accountEntities = new List<AccountEntity>();
        await _retryPolicy.ExecuteAsync(async () =>
        {
            accountEntities = await _accountContext.AccountEntity.Where(a => accountIds.Contains(a.AccountId)).ToListAsync();
        });

        return accountEntities;
    }

    private async Task<IEnumerable<int>> CheckExistingAccountsAsync(IEnumerable<int> accountIds)
    {
        var missingIds = new List<int>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var dbIds = await _accountContext.AccountEntity.Select(a => a.AccountId).ToListAsync();
            missingIds = accountIds.Except(dbIds).ToList();
        });

        return missingIds;
    }

    private async Task<DelegationEntity> GetDelegationAsync(int delegationId)
    {
        DelegationEntity? tDelegation = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            tDelegation = await _accountContext.DelegationEntity
                .Include(d => d.Account)
                .FirstOrDefaultAsync(d => d.DelegationId == delegationId);
        });

        if (tDelegation == null)
        {
            throw new NotFoundException(Errors.NotFoundDelegationCode, string.Format(Errors.NotFoundDelegationMessage, delegationId));
        }

        return tDelegation;
    }

    private async Task<IEnumerable<RoleEntity>> GetRoleForDelegationAsync(IEnumerable<int> accountIds, int contactId)
    {
        var roles = Enumerable.Empty<RoleEntity>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            roles = await _accountContext.RoleEntity
                .Where(r => accountIds.Contains(r.AccountId) && r.ContactId == contactId && r.IsDelegation == true)
                .ToListAsync();
        });

        return roles;
    }
}
