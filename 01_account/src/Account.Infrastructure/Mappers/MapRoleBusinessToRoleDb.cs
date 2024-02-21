// <copyright file="MapRoleBusinessToRoleDb.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Net;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapRoleBusinessToRoleDb
    {
        public static TRole MapRoleToRoleDb(this Role role)
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
