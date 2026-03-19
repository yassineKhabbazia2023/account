// <copyright file="RegistryAccountCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryAccountCreatedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryAccountCreatedEventHandler> _logger;
    private readonly IRegistryAccountEventRepository _accountEventRepository;
    private readonly IAccountEventPublisher _accountEventPublisher;

    public RegistryAccountCreatedEventHandler(
        ILogger<RegistryAccountCreatedEventHandler> logger,
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

        var @event = JsonConvert.DeserializeObject<RegistryAccountCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, Id: {Id}",
       @event?.EventType,
       @event?.Data?.AccountGlobalUniqueIdentifier);

        if (@event?.Data == null || @event.Data.AccountGlobalUniqueIdentifier == default)
        {
            return;
        }

        if (!await _accountEventRepository.DoesAccountExistAsync(@event.Data.AccountGlobalUniqueIdentifier))
        {
            var createdAccount = await _accountEventRepository.CreateAccountAsync(@event.Data);
            _logger.LogInformation("L'entité avec l'identifiant suivant: {AccountId} vient d'être créée.", createdAccount.AccountId);

            await _accountEventPublisher.PublishAccountCreatedEventAsync(createdAccount);
        }
        else
        {
            _logger.LogWarning("L'entité avec l'identifiant global suivant: {AccountId} existe déjà.", @event.Data.AccountGlobalUniqueIdentifier);
        }
    }
}
