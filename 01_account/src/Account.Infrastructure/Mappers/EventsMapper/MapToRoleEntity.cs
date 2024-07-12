// <copyright file="MapToRoleEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
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

        public static IEnumerable<CreateRoleRequest> ToCreateRoleRequests(this IEnumerable<RoleEntity>? roleEntities)
        {
            return roleEntities?.Select(x => x.ToCreateRoleRequest()) ?? Enumerable.Empty<CreateRoleRequest>();
        }
    }
}
