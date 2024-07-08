// <copyright file="RoleEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Interfaces;

namespace Pulse.Account.Infrastructure.Providers
{
    public class RoleEventPublisher : IRoleEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IContactRepository _contactRepository;
        private readonly IAccountRepository _accountRepository;

        public RoleEventPublisher(IEventPublisher eventPublisher, IContactRepository contactRepository, IAccountRepository accountRepository)
        {
            _eventPublisher = eventPublisher;
            _contactRepository = contactRepository;
            _accountRepository = accountRepository;
        }

        public async Task PublishRoleCreatedEventAsync(CreateRoleRequest roleRequest)
        {
            if (roleRequest is null)
            {
                return;
            }

            roleRequest.AccountGlobalUniqueId = roleRequest.AccountGlobalUniqueId ??
                (await _accountRepository.GetAccountAsync(roleRequest.AccountId)) !.AccountGlobalUniqueId;

            roleRequest.ContactGlobalUniqueId = roleRequest.ContactGlobalUniqueId ??
                (await _contactRepository.GetContactAsync(roleRequest.ContactId)).ContactGlobalUniqueId;

            var data = new RoleCreatedEventData
            {
                AccountId = roleRequest.AccountId,
                ContactId = roleRequest.ContactId,
                IsDelegation = roleRequest.IsDelegation,
                IsFavorite = roleRequest.IsFavorite,
                IsSignatory = roleRequest.IsSignatory,
                AccountGlobalUniqueId = (Guid)roleRequest.AccountGlobalUniqueId!,
                ContactGlobalUniqueId = (Guid)roleRequest.ContactGlobalUniqueId!
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
            var contact = await _contactRepository.GetContactAsync(contactId);

            var account = await _accountRepository.GetAccountAsync(accountId);

            var data = new RoleDeletedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
                AccountGlobalUniqueId = account!.AccountGlobalUniqueId,
                ContactGlobalUniqueId = contact!.ContactGlobalUniqueId,
            };

            await _eventPublisher.PublishAsync(new RoleDeletedEvent(data));
        }
    }
}
