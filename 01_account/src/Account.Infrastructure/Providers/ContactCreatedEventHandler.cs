// <copyright file="ContactCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Microsoft.IdentityModel.Tokens;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactCreatedEventHandler : IEventHandler
{
    private readonly ILogger<ContactCreatedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;
    private readonly IRoleEventRepository _roleEventRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;

    public ContactCreatedEventHandler(
        ILogger<ContactCreatedEventHandler> logger,
        IContactEventRepository contactEventRepository,
        IRoleEventRepository roleEventRepository,
        IRoleEventPublisher roleEventPublisher)
    {
        _logger = logger;
        _contactEventRepository = contactEventRepository;
        _roleEventRepository = roleEventRepository;
        _roleEventPublisher = roleEventPublisher;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var contactEvent = JsonConvert.DeserializeObject<ContactCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            contactEvent?.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent?.Data?.ContactId <= 0)
        {
            return;
        }

        var contactEntity = contactEvent!.Data.ToContactEntity();

        await _contactEventRepository.CreateContactAsync(contactEntity!);

        if (!contactEvent.Data.AccountNumber.IsNullOrEmpty())
        {
            var role = await _roleEventRepository.CreateRoleForNewContact(contactEntity.ContactId, contactEvent.Data.AccountNumber!);
            if(role != null)
            {
                await _roleEventPublisher.PublishRoleCreatedEventAsync(role);
            }
        }

        _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être crée.", contactEntity.ContactId);
    }
}
