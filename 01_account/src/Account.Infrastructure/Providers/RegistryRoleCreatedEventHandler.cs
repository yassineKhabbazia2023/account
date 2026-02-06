// <copyright file="RegistryRoleCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Enum;
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
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly IRoleRepository _roleRepository;

    public RegistryRoleCreatedEventHandler(
        ILogger<RegistryRoleCreatedEventHandler> logger,
        IRegistryRoleEventRepository roleEventRepository,
        IRoleEventPublisher roleEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        IRoleRepository roleRepository)
    {
        _logger = logger;
        _roleEventRepository = roleEventRepository;
        _roleEventPublisher = roleEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _roleRepository = roleRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var @event = JsonConvert.DeserializeObject<RegistryRoleCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, AccountId: {AccountId}, ContactId: {ContactId} | AccountGuid: {AccountGuid}, ContactGuid: {ContactGuid}, IsCustomerRelation: {IsCustomerRelation}, RegistryApproverEmail: {RegistryApproverEmail},ContactFlagPortailFactures: {ContactFlagPortailFactures}",
            @event?.EventType,
            @event?.Data?.AccountId,
            @event?.Data?.ContactId,
            @event?.Data?.AccountGuid,
            @event?.Data?.ContactGuid,
            @event?.Data?.RegistryApproverEmail,
            @event?.Data?.IsCustomerRelation,
            @event?.Data?.ContactFlagPortailFactures);

        if (@event?.Data == null)
        {
            return;
        }

        var accountId = @event.Data.AccountId ?? 0;
        var contactId = @event.Data.ContactId ?? 0;
        var registryApproverEmail = @event.Data.RegistryApproverEmail ?? string.Empty;
        bool? isCustomerRelation = null;

        // Vérifier l'existence des Guids
        if (@event.Data.AccountGuid.HasValue && @event.Data.ContactGuid.HasValue)
        {
            accountId = await _roleEventRepository.GetAccountIdByGuidAsync((Guid)@event.Data.AccountGuid);

            var contact = await _roleEventRepository.GetContactByGuidAsync((Guid)@event.Data.ContactGuid);
            contactId = contact.ContactId;
            isCustomerRelation = contact.Type == ContactType.Customer.ToString() ? null : @event?.Data?.IsCustomerRelation;
        }

        await _roleEventRepository.CheckExistingAccountAndContactAsync(accountId, contactId);

        var existingRole = await _roleRepository.GetContactRoleAsync(accountId, contactId);
        if (existingRole != null)
        {
            // Cette logique gère les mises à jour de ContactFlagPortailFactures envoyées par Registry.
            // La comparaison bool? est volontaire : null et false sont considérés différents afin de synchroniser explicitement la valeur.
            if (existingRole.ContactFlagPortailFactures != @event!.Data.ContactFlagPortailFactures)
            {
                var updatedRole = await _roleEventRepository.UpdateRoleContactFlagPortailFacturesAsync(accountId, contactId, @event!.Data.ContactFlagPortailFactures);
                if (updatedRole == null)
                {
                    _logger.LogWarning("Le role avec l'identifiant suivant: AccountId: {AccountId} - ContactId: {ContactId} n'a pas pu être mis à jour.", accountId, contactId);
                    return;
                }

                _logger.LogInformation("Le role avec l'identifiant suivant: AccountId: {AccountId} - ContactId: {ContactId} vient d'être mise à jour.", updatedRole.AccountId, updatedRole.ContactId);

                await _roleEventPublisher.PublishRoleCreatedEventAsync(updatedRole);
            }
            else
            {
                _logger.LogWarning(string.Format(Errors.BadRequestExistingRoleMessage, contactId, accountId));
            }
        }
        else
        {
            var createdRole = await _roleEventRepository.CreateRoleAsync(@event!.Data, accountId, contactId, isCustomerRelation);
            _logger.LogInformation("Le role avec l'identifiant suivant: AccountId: {AccountId} - ContactId: {ContactId} vient d'être mise à jour.", createdRole.AccountId, createdRole.ContactId);

            await _roleEventPublisher.PublishRoleCreatedEventAsync(createdRole, @event.Data.SubRole);

            await _historyEventPublisher.PublishHistoryCreatedEventAsync(registryApproverEmail, contactId, accountId);
        }
    }
}
