// <copyright file="RegistryAcccountUpdatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryAccountUpdatedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryAccountUpdatedEventHandler> _logger;
    private readonly IRegistryAccountEventRepository _accountEventRepository;
    private readonly IAccountEventPublisher _accountEventPublisher;

    public RegistryAccountUpdatedEventHandler(
        ILogger<RegistryAccountUpdatedEventHandler> logger,
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

        var @event = JsonConvert.DeserializeObject<RegistryAccountUpdatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, Id: {Id}",
            @event?.EventType,
            @event?.Data?.Id);

        if (@event?.Data == null || @event?.Data.Id == default(Guid))
        {
            return;
        }

        var updatedAccount = await _accountEventRepository.UpdateAccountAsync(@event!.Data);
        _logger.LogInformation("L'entité avec l'identifiant suivant: {AccountId} vient d'être mise à jour.", updatedAccount.AccountId);

        await _accountEventPublisher.PublishAccountUpdatedEventAsync(updatedAccount);
    }
}
