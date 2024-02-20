// <copyright file="MapBusinessToDb.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Net;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapBusinessToDb
    {
        public static TRole MapRoleBusinessToRoleDb(this Role role)
        {
            if (role == null)
            {
                throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotNullException);
            }

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
