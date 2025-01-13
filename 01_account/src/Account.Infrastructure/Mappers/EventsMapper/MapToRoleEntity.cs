// <copyright file="MapToRoleEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Mappers.EventsMapper;

public static class MapToRoleEntity
{
    public static RoleEntity ToRoleEntity(this RegistryRoleCreatedEventData eventData, int accountId, int contactId)
    {
        if (eventData == null)
        {
            return null!;
        }

        return new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = eventData.IsFavorite,
            IsSignatory = eventData.RoleSignatory,
        };
    }

    public static IEnumerable<RoleEntity> ToRoleEntities(this IEnumerable<CreateRoleRequest>? requests)
    {
        return requests?.Select(r => r.ToRoleEntity()) ?? Enumerable.Empty<RoleEntity>();
    }

    public static RoleEntity ToRoleEntity(this CreateRoleRequest request)
    {
        if (request == null)
        {
            return null!;
        }

        return new RoleEntity
        {
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            IsFavorite = request.IsFavorite,
            IsSignatory = request.IsSignatory,
            IsDelegation = request.IsDelegation,
        };
    }

    public static CreateRoleRequest ToCreateRoleRequest(this RoleEntity roleEntity)
    {
        if(roleEntity == null)
        {
            return null!;
        }

        return new CreateRoleRequest
        {
            AccountId = roleEntity.AccountId,
            AccountGlobalUniqueId = roleEntity.Account?.AccountGlobalUniqueId,
            ContactId = roleEntity.ContactId,
            ContactGlobalUniqueId = roleEntity.Contact?.ContactGlobalUniqueId,
            IsFavorite = roleEntity.IsFavorite,
            IsSignatory = roleEntity.IsSignatory,
            IsDelegation = roleEntity.IsDelegation,
        };
    }

    public static Role ToRole(this RoleCreatedEventData source)
    {
        if (source == null)
        {
            return null!;
        }

        return new Role
        {
            ContactId = source.ContactId,
            AccountId = source.AccountId,
            IsDelegation = source.IsDelegation,
            IsFavorite = source.IsFavorite,
            IsSignatory = source.IsSignatory
        };
    }

    public static IEnumerable<CreateRoleRequest> ToCreateRoleRequests(this IEnumerable<RoleEntity>? roleEntities, int delegatorId)
    {
        return roleEntities?.Select(x => x.ToCreateRoleRequest(delegatorId)) ?? Enumerable.Empty<CreateRoleRequest>();
    }

    private static CreateRoleRequest ToCreateRoleRequest(this RoleEntity roleEntity, int delegatorId)
    {
        var role = roleEntity.ToCreateRoleRequest();
        role.DelegatorId = delegatorId;

        return role;
    }
}
