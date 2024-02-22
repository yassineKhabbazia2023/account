// <copyright file="MapRoleBusinessToRoleDb.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapRoleBusinessToRoleDb
    {
        public static TRole MapRoleToRoleDb(this CreateRole role)
        {
            return new TRole()
            {
                AccountId = role.AccountId,
                ContactId = role.ContactId,
                IsFavorite = role.IsFavorite,
                IsSignatory = role.IsSignatory
            };
        }
    }
}
