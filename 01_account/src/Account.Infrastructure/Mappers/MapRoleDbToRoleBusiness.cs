// <copyright file="MapRoleDbToRoleBusiness.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers;

public static class MapRoleDbToRoleBusiness
{
    public static Role? MapToRole(this RoleEntity role)
    {
        return role == null ? null! : new Role
        {
            AccountId = role.AccountId,
            ContactId = role.ContactId,
            IsFavorite = role.IsFavorite,
            IsSignatory = role.IsSignatory,
            IsDelegation = role.IsDelegation,
            ActionLevel = role.ActionLevel,
            IsCustomerRelation = role.IsCustomerRelation,
            ContactFlagPortailFactures = role.ContactFlagPortailFactures,
        };
    }

    public static IEnumerable<Role> MapToRoles(this IEnumerable<RoleEntity> roleEntities)
    {
        return roleEntities?.Select(r => r.MapToRole() !) ?? Enumerable.Empty<Role>();
    }
}
