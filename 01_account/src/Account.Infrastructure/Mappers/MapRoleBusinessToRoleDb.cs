// <copyright file="MapRoleBusinessToRoleDb.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapRoleBusinessToRoleDb
    {
        public static IEnumerable<RoleEntity> MapRolesToRoleDb(this IEnumerable<CreateRoleRequest> role)
        {
            return role?.Select(r => r.MapRoleToRoleDb()) ?? Enumerable.Empty<RoleEntity>();
        }

        public static RoleEntity MapRoleToRoleDb(this CreateRoleRequest role)
        {
            return role == null ? null! : new RoleEntity
            {
                AccountId = role.AccountId,
                ContactId = (int)role.ContactId!,
                IsFavorite = role.IsFavorite,
                IsSignatory = role.IsSignatory,
                IsDelegation = role.IsDelegation,
                IsCustomerRelation = false,
            };
        }

        public static RoleEntity MapRoleToRoleDb(this Role role)
        {
            return role == null ? null! : new RoleEntity
            {
                AccountId = role.AccountId,
                ContactId = role.ContactId,
                IsFavorite = role.IsFavorite,
                IsSignatory = role.IsSignatory,
                IsDelegation = role.IsDelegation,
                IsCustomerRelation = role.IsCustomerRelation,
            };
        }

        public static IEnumerable<RoleEntity> MapRolesToRolesDb(this IEnumerable<Role>? roles)
        {
            return roles?.Select(r => r.MapRoleToRoleDb()) ?? Enumerable.Empty<RoleEntity>();
        }
    }
}
