// <copyright file="ContactRemovedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactRemovedEventHandler : IEventHandler
{
    private readonly ILogger<ContactRemovedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;

    public ContactRemovedEventHandler(
        ILogger<ContactRemovedEventHandler> logger,
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

        var contactEvent = JsonConvert.DeserializeObject<ContactRemovedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            contactEvent?.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent?.Data?.ContactId <= 0)
        {
            return;
        }

        await _contactEventRepository.RemoveContactAsync(contactEvent!.Data.ContactId);

        _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être supprimé.", contactEvent!.Data.ContactId);
    }
}
