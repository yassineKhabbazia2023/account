// <copyright file="ContactRevokedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactRevokedEventHandler : IEventHandler
{
    private readonly ILogger<ContactRevokedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;

    public ContactRevokedEventHandler(
        ILogger<ContactRevokedEventHandler> logger,
        IContactEventRepository contactEventRepository)
    {
        _logger = logger;
        _contactEventRepository = contactEventRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var contactEvent = JsonConvert.DeserializeObject<ContactRevokedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            contactEvent?.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent?.Data?.ContactId <= 0)
        {
            return;
        }

        await _contactEventRepository.RevokeContactAsync(contactEvent!.Data.ContactId);

        _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être revoqué.", contactEvent!.Data.ContactId);
    }
}
