// <copyright file="MapRoleBusinessToRoleBusiness.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Infrastructure.Mappers;

public static class MapRoleBusinessToRoleBusiness
{
    public static Role MapCreateRoleRequestToRole(this CreateRoleRequest role)
    {
        return role == null ? null! : new Role
        {
            AccountId = role.AccountId,
            ContactId = role.ContactId,
            IsFavorite = role.IsFavorite,
            IsSignatory = role.IsSignatory,
            IsDelegation = role.IsDelegation,
        };
    }
}
