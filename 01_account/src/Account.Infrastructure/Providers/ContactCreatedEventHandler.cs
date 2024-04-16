// <copyright file="ContactCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactCreatedEventHandler : IEventHandler
{
    private readonly ILogger<ContactCreatedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;

    public ContactCreatedEventHandler(
        ILogger<ContactCreatedEventHandler> logger,
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

        var contactEvent = JsonConvert.DeserializeObject<ContactCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {0}, contactId: {1}",
            contactEvent!.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent?.Data?.ContactId <= 0)
        {
            return;
        }

        var contactEntity = contactEvent!.Data.ToContactEntity();

        await _contactEventRepository.CreateContactAsync(contactEntity!);

        _logger.LogInformation("Le contact avec l'identifiant: {contactId} vient d'être crée", contactEntity.ContactId);
    }
}
