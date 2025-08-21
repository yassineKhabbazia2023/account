// <copyright file="ContactRemovedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactRemovedEventHandler : IEventHandler
{
    private readonly ILogger<ContactRemovedEventHandler> _logger;
    private readonly IRoleEventPublisher _roleEventPublisher;
    private readonly IContactEventRepository _contactEventRepository;
    private readonly IRoleEventRepository _roleEventRepository;
    private readonly IDelegationEventRepository _delegationEventRepository;

    public ContactRemovedEventHandler(
        ILogger<ContactRemovedEventHandler> logger,
        IRoleEventPublisher roleEventPublisher,
        IContactEventRepository contactEventRepository,
        IRoleEventRepository roleEventRepository,
        IDelegationEventRepository delegationEventRepository)
    {
        _logger = logger;
        _roleEventPublisher = roleEventPublisher;
        _contactEventRepository = contactEventRepository;
        _roleEventRepository = roleEventRepository;
        _delegationEventRepository = delegationEventRepository;
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
        await _delegationEventRepository.DeleteContactDelegationsAsync(contactEvent!.Data.ContactId);
        var rolesToDelete = await _roleEventRepository.DeleteContactRolesAsync(contactEvent!.Data.ContactId);

        var publishTasks = rolesToDelete.Select(role =>
            _roleEventPublisher.PublishRoleDeletedEventAsync(role.AccountId, role.ContactId));

        await Task.WhenAll(publishTasks);

        _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être supprimé.", contactEvent!.Data.ContactId);
    }
}
