// <copyright file="RegistryRoleCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
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
    private readonly ILabelService _labelService;
    private readonly IRoleLabelService _roleLabelService;

    public RegistryRoleCreatedEventHandler(
        ILogger<RegistryRoleCreatedEventHandler> logger,
        IRegistryRoleEventRepository roleEventRepository,
        IRoleEventPublisher roleEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        IRoleRepository roleRepository,
        ILabelService labelService,
        IRoleLabelService roleLabelService)
    {
        _logger = logger;
        _roleEventRepository = roleEventRepository;
        _roleEventPublisher = roleEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _roleRepository = roleRepository;
        _labelService = labelService;
        _roleLabelService = roleLabelService;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var @event = JsonConvert.DeserializeObject<RegistryRoleCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, AccountId: {AccountId}, ContactId: {ContactId} | AccountGuid: {AccountGuid}, ContactGuid: {ContactGuid}, IsCustomerRelation: {IsCustomerRelation}, RegistryApproverEmail: {RegistryApproverEmail}, ContactFlagPortailFactures: {ContactFlagPortailFactures}, Description: {Description}",
            @event?.EventType,
            @event?.Data?.AccountId,
            @event?.Data?.ContactId,
            @event?.Data?.AccountGuid,
            @event?.Data?.ContactGuid,
            @event?.Data?.IsCustomerRelation,
            @event?.Data?.RegistryApproverEmail,
            @event?.Data?.ContactFlagPortailFactures,
            @event?.Data?.Description);

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
            }
            else
            {
                _logger.LogWarning(string.Format(Errors.BadRequestExistingRoleMessage, contactId, accountId));
            }

            if (existingRole.IsSignatory != @event!.Data.RoleSignatory)
            {
                var updated = await _roleEventRepository.UpdateRoleIsSignatoryAsync(accountId, contactId, @event!.Data.RoleSignatory);
                if (!updated)
                {
                    _logger.LogWarning("Le role avec l'identifiant suivant: AccountId: {AccountId} - ContactId: {ContactId} n'a pas pu être mis à jour (IsSignatory).", accountId, contactId);
                    return;
                }
            }

            // Rattrapage : si le rôle existant n'a pas encore IsCustomerRelation à true et que la description correspond à CLP ou AM
            if (@event!.Data.IsCustomerRelation && existingRole.IsCustomerRelation != true)
            {
                var actionLevel = ActionLevelHelper.SetupActionLevel((int)ActionLevelType.NotAssigned, true, false);
                var updatedRelation = await _roleEventRepository.UpdateRoleIsCustomerRelationAsync(accountId, contactId, true, actionLevel);
                if (updatedRelation == null)
                {
                    _logger.LogWarning("Échec de la mise à jour IsCustomerRelation pour AccountId: {AccountId} - ContactId: {ContactId}", accountId, contactId);
                }
            }

            // Re-lire l'état courant du rôle après les éventuelles mises à jour et publier une seule fois
            var currentRole = await _roleRepository.GetContactRoleAsync(accountId, contactId);
            await _roleEventPublisher.PublishRoleCreatedEventAsync(currentRole!.ToCreateRoleRequest());

            try
            {
                await AssignRoleLabelFromDescriptionAsync(@event!.Data.Description, accountId, contactId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du label pour le rôle existant AccountId: {AccountId} - ContactId: {ContactId}, Description: {Description}", accountId, contactId, @event!.Data.Description);
            }
        }
        else
        {
            var createdRole = await _roleEventRepository.CreateRoleAsync(@event!.Data, accountId, contactId, isCustomerRelation);
            _logger.LogInformation("Le role avec l'identifiant suivant: AccountId: {AccountId} - ContactId: {ContactId} vient d'être mise à jour.", createdRole.AccountId, createdRole.ContactId);

            await _roleEventPublisher.PublishRoleCreatedEventAsync(createdRole, @event.Data.SubRole);

            await _historyEventPublisher.PublishHistoryCreatedEventAsync(registryApproverEmail, contactId, accountId);

            try
            {
                await AssignRoleLabelFromDescriptionAsync(@event.Data.Description, accountId, contactId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du label pour le nouveau rôle AccountId: {AccountId} - ContactId: {ContactId}, Description: {Description}", accountId, contactId, @event.Data.Description);
            }
        }
    }

    private async Task AssignRoleLabelFromDescriptionAsync(string? description, int accountId, int contactId)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        var labels = (await _labelService.GetLabelsAsync(new Pagination())).Items ?? Enumerable.Empty<Label>();
        var label = labels.FirstOrDefault(l => l.Code.Equals(description, StringComparison.OrdinalIgnoreCase));

        if (label == null)
        {
            _logger.LogWarning("Label avec le code {Description} introuvable en base pour le rôle AccountId: {AccountId} - ContactId: {ContactId}", description, accountId, contactId);
            return;
        }

        if (await _roleLabelService.HasRoleLabel(contactId, accountId, label.LabelId))
        {
            _logger.LogWarning("Le label {Description} est déjà affecté au rôle AccountId: {AccountId} - ContactId: {ContactId}", description, accountId, contactId);
            return;
        }

        await _roleLabelService.RevokeExclusiveLabelAsync(accountId, label.LabelId, label.Code);

        await _roleLabelService.AddRoleLabelAsync(new RoleLabel
        {
            AccountId = accountId,
            ContactId = contactId,
            LabelId = label.LabelId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = contactId,
        });

        _logger.LogInformation("Le libellé {Description} a été ajouté sur le rôle AccountId {AccountId}/ContactId {ContactId}", description, accountId, contactId);
    }
}
