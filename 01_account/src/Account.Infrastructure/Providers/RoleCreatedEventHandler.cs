// <copyright file="RoleCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class RoleCreatedEventHandler : IEventHandler
{
    private readonly ILogger<RoleCreatedEventHandler> _logger;
    private readonly IRoleEventRepository _roleEventRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;

    public RoleCreatedEventHandler(ILogger<RoleCreatedEventHandler> logger,
        IRoleEventRepository roleEventRepository,
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

        var roleEvent = JsonConvert.DeserializeObject<RoleCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event n°{EventId} type: {EventType}, contactId: {ContactId}, accountId: {AccountId}",
                roleEvent?.EventId,
                roleEvent?.EventType,
                roleEvent?.Data?.ContactId,
                roleEvent?.Data?.AccountId);

        if (roleEvent?.Data == null || roleEvent.Data.ContactId <= 0 || roleEvent.Data.AccountId < -1)
        {
            return;
        }

        var role = roleEvent!.Data.ToRole();

        var rolesCreated = await _roleEventRepository.CreateRoleForAutomaticDelegationsAsync(role.ContactId, role.AccountId);

        _logger.LogInformation("L'event n°{EventId} type: {EventType}, contactId: {ContactId}, accountId: {AccountId} a été consommé",
                roleEvent?.EventId,
                roleEvent?.EventType,
                roleEvent?.Data?.ContactId,
                roleEvent?.Data?.AccountId);

        if (rolesCreated.Any())
        {
            foreach (var roleCreated in rolesCreated)
            {
                _logger.LogInformation("Le role de contact: {ContactId}, account: {AccountId} vient d'être crée.", roleCreated.ContactId, roleCreated.AccountId);

                await PublishRoleCreatedEvent(roleCreated);
            }
        }
    }

    private async Task PublishRoleCreatedEvent(CreateRoleRequest role)
    {
        _logger.LogInformation("RoleService: Start send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);

        await _roleEventPublisher.PublishRoleCreatedEventAsync(role);

        _logger.LogInformation("RoleService: End send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);
    }
}
