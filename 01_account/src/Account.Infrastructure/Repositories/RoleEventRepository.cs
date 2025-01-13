// <copyright file="RoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Repositories;

public class RoleEventRepository : IRoleEventRepository
{
    private readonly AccountContext _accountContext;

    public RoleEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();
    }

    public async Task<IEnumerable<CreateRoleRequest>> CreateRoleForAutomaticDelegationsAsync(int delegatorId, int accountId)
    {
        var rolesToCreate = await GetAutomaticDelegations(delegatorId, accountId);

        var roleEntities = rolesToCreate.ToRoleEntities();

        if (roleEntities.Any())
        {
            await _accountContext.RoleEntity.AddRangeAsync(roleEntities);
        }

        await _accountContext.SaveChangesAsync();

        return rolesToCreate;
    }

    public async Task<IEnumerable<RoleEntity>> DeleteContactRolesAsync(int contactId)
    {
        var rolesToDelete = await _accountContext.RoleEntity.Where(r => r.ContactId == contactId).ToListAsync();
        rolesToDelete.ForEach(r =>
                     {
                         _accountContext.Entry(r).State = EntityState.Deleted;
                     });

        await _accountContext.SaveChangesAsync();

        return rolesToDelete;
    }

    private async Task<IEnumerable<CreateRoleRequest>> GetAutomaticDelegations(int delegatorId, int accountId)
    {
        var rolesToCreate = new List<CreateRoleRequest>();

        var delegations = await _accountContext.DelegationEntity
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegatee)
                                        .ThenInclude(delegatee => delegatee.RoleEntity)
                                        .Where(d => d.DelegatorId == delegatorId
                                            && !d.Status.Equals(DelegationStatus.Disabled.ToString().ToLower())
                                            && d.IsAutomaticDelegation)
                                        .ToListAsync();

        var account = await _accountContext.AccountEntity.FirstOrDefaultAsync(a => a.AccountId == accountId);

        delegations.ForEach(d =>
        {
            if (!d.Account.Any(a => a.AccountId == accountId))
            {
                List<AccountEntity> newDelegationAccounts = d.Account.ToList();
                newDelegationAccounts.Add(account!);
                d.Account = newDelegationAccounts;
            }

            if (!d.Delegatee.RoleEntity.Any(r => r.AccountId == accountId))
            {
                rolesToCreate.Add(CreateRoleForDelegation(d.DelegateeId, d.Delegatee.ContactGlobalUniqueId, accountId, account!.AccountGlobalUniqueId));
            }
        });

        return rolesToCreate;
    }

    private CreateRoleRequest CreateRoleForDelegation(int contactId, Guid? contactGlobalUniqueId, int accountId, Guid accountGlobalUniqueId)
    {
        var rolesCreated = new CreateRoleRequest
        {
            ContactId = contactId,
            ContactGlobalUniqueId = contactGlobalUniqueId,
            AccountId = accountId,
            AccountGlobalUniqueId = accountGlobalUniqueId,
            IsSignatory = false,
            IsFavorite = false,
            IsDelegation = true
        };

        return rolesCreated;
    }

    public async Task<CreateRoleRequest?> CreateRoleForNewContactAsync(int contactId, int accountId)
    {
        var role = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsSignatory = false,
            IsDelegation = false,
            IsFavorite = false,
        };

        _accountContext.RoleEntity.Add(role);
        await _accountContext.SaveChangesAsync();

        return new CreateRoleRequest
        {
            ContactId = contactId,
            AccountId = accountId,
            IsSignatory = false,
            IsFavorite = false,
            IsDelegation = true
        };
    }

    public async Task<bool> DoesRoleExistAsync(int contactId, int accountId)
    {
        return await _accountContext.RoleEntity.AnyAsync(r => r.ContactId == contactId && r.AccountId == accountId);
    }
}
