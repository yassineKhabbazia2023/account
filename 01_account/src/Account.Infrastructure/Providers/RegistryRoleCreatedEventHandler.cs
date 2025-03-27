// <copyright file="RegistryRoleCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryRoleCreatedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryRoleCreatedEventHandler> _logger;
    private readonly IRegistryRoleEventRepository _roleEventRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;
    private readonly IRoleRepository _roleRepository;

    public RegistryRoleCreatedEventHandler(
        ILogger<RegistryRoleCreatedEventHandler> logger,
        IRegistryRoleEventRepository roleEventRepository,
        IRoleEventPublisher roleEventPublisher,
        IRoleRepository roleRepository)
    {
        _logger = logger;
        _roleEventRepository = roleEventRepository;
        _roleEventPublisher = roleEventPublisher;
        _roleRepository = roleRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var @event = JsonConvert.DeserializeObject<RegistryRoleCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, AccountId: {AccountId}, ContactId: {ContactId}",
            @event?.EventType,
            @event?.Data?.AccountId,
            @event?.Data?.ContactId);

        if (@event?.Data == null || @event?.Data.AccountId == null || @event?.Data.ContactId == null)
        {
            return;
        }

        var accountId = @event!.Data.AccountId;
        var contactId = @event!.Data.ContactId;
        await _roleEventRepository.CheckExistingAccountAndContactAsync(accountId, contactId);

        if (await _roleRepository.GetContactRoleAsync(accountId, contactId) != null)
        {
            _logger.LogWarning(string.Format(Errors.BadRequestExistingRoleMessage, contactId, accountId));
        }
        else
        {
            var createdRole = await _roleEventRepository.CreateRoleAsync(@event!.Data);
            _logger.LogInformation("Le role avec l'identifiant suivant: AccountId: {AccountId} - ContactId: {ContactId} vient d'être mise à jour.", createdRole.AccountId, createdRole.ContactId);

            await _roleEventPublisher.PublishRoleCreatedEventAsync(createdRole);
        }
    }
}
