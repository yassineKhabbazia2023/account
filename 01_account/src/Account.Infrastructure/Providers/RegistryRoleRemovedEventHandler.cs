// <copyright file="RegistryRoleRemovedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers
{
    public class RegistryRoleRemovedEventHandler : IEventHandler
    {
        private readonly ILogger<RegistryRoleRemovedEventHandler> _logger;
        private readonly IRegistryRoleEventRepository _roleEventRepository;
        private readonly IRoleEventPublisher _roleEventPublisher;

        public RegistryRoleRemovedEventHandler(
            ILogger<RegistryRoleRemovedEventHandler> logger,
            IRegistryRoleEventRepository roleEventRepository,
            IRoleEventPublisher roleEventPublisher)
        {
            _logger = logger;
            _roleEventRepository = roleEventRepository;
            _roleEventPublisher = roleEventPublisher;
        }

        public async Task HandleAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var @event = JsonConvert.DeserializeObject<RegistryRoleRemovedEvent>(message);
            _logger.LogInformation("Consommation de l'event type: {EventType}, AccountId: {AccountId}, ContactId: {ContactId}",
                @event?.EventType,
                @event?.Data?.AccountId,
                @event?.Data?.ContactId);

            if (@event?.Data == null || @event?.Data.AccountId == default(Guid) || @event?.Data.ContactId == default(Guid))
            {
                return;
            }

            var (accountId, contactId) = await _roleEventRepository.RemoveRoleAsync(@event!.Data);
            _logger.LogInformation("L'entité avec l'identifiant suivant: AccountId: {AccountId}, ContactId: {ContactId} vient d'être mise à jour.", accountId, contactId);

            await _roleEventPublisher.PublishRoleDeletedEventAsync(accountId, contactId);
        }
    }
}
