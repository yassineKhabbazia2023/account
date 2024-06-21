// <copyright file="RegistryAccountRemovedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryAccountRemovedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryAccountRemovedEventHandler> _logger;
    private readonly IRegistryAccountEventRepository _accountEventRepository;
    private readonly IAccountEventPublisher _accountEventPublisher;

    public RegistryAccountRemovedEventHandler(
        ILogger<RegistryAccountRemovedEventHandler> logger,
        IRegistryAccountEventRepository accountEventRepository,
        IAccountEventPublisher accountEventPublisher)
    {
        _logger = logger;
        _accountEventRepository = accountEventRepository;
        _accountEventPublisher = accountEventPublisher;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var @event = JsonConvert.DeserializeObject<RegistryAccountRemovedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, Id: {Id}",
            @event?.EventType,
            @event?.Data?.AccountGlobalUniqueIdentifier);

        if (@event?.Data == null || @event.Data.AccountGlobalUniqueIdentifier == default(Guid))
        {
            return;
        }

        var removedAccountId = await _accountEventRepository.RemoveAccountAsync(@event.Data.AccountGlobalUniqueIdentifier);
        _logger.LogInformation("L'entité avec l'identifiant suivant: {AccountId} vient d'être supprimée.", removedAccountId);

        await _accountEventPublisher.PublishAccountRemovedEventAsync(removedAccountId);
    }
}
