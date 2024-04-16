// <copyright file="RoleEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Infrastructure.Providers
{
    public class RoleEventPublisher : IRoleEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;

        public RoleEventPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task PublishRoleCreatedEventAsync(CreateRoleRequest roleRequest)
        {
            if (roleRequest is null)
            {
                return;
            }

            var data = new RoleCreatedEventData
            {
                AccountId = roleRequest.AccountId,
                ContactId = roleRequest.ContactId,
                IsDelegation = roleRequest.IsDelegation,
                IsFavorite = roleRequest.IsFavorite,
                IsSignatory = roleRequest.IsSignatory,
            };

            await _eventPublisher.PublishAsync(new RoleCreatedEvent(data));
        }

        public async Task PublishRoleUpdatedEventAsync(int accountId, int contactId, bool isSignatory)
        {
            var data = new RoleUpdatedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
                IsSignatory = isSignatory,
            };
            await _eventPublisher.PublishAsync(new RoleUpdatedEvent(data));
        }

        public async Task PublishRoleDeletedEventAsync(int accountId, int contactId)
        {
            var data = new RoleDeletedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
            };

            await _eventPublisher.PublishAsync(new RoleDeletedEvent(data));
        }
    }
}
