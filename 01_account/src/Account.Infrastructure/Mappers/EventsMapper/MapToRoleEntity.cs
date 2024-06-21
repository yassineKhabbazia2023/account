// <copyright file="MapToRoleEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Mappers.EventsMapper
{
    public static class MapToRoleEntity
    {
        public static RoleEntity ToRoleEntity(this RegistryRoleCreatedEventData eventData, AccountContext accountContext)
        {
            if (eventData == null)
            {
                return null!;
            }

            var account = accountContext.AccountEntity.FirstOrDefault(x => x.AccountGlobalUniqueId == eventData.AccountId);
            var contact = accountContext.ContactEntity.FirstOrDefault(x => x.ContactGlobalUniqueId == eventData.ContactId);

            if (account == null || contact == null)
            {
                throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, eventData.ContactId, eventData.AccountId));
            }

            return new RoleEntity
            {
                AccountId = account.AccountId,
                ContactId = contact.ContactId,
                IsFavorite = eventData.IsFavorite,
                IsSignatory = eventData.RoleSignatory,
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
                ContactId = roleEntity.ContactId,
                IsFavorite = roleEntity.IsFavorite,
                IsSignatory = roleEntity.IsSignatory,
            };
        }
    }
}
