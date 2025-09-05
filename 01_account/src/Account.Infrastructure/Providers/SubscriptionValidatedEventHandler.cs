// <copyright file="SubscriptionValidatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class SubscriptionValidatedEventHandler : IEventHandler
{
    private readonly ILogger<SubscriptionValidatedEventHandler> _logger;
    private readonly IRoleRepository _roleRepository;
    private readonly ILabelService _labelService;
    private readonly IRoleLabelRepository _roleLabelRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;

    public SubscriptionValidatedEventHandler(ILogger<SubscriptionValidatedEventHandler> logger, IRoleRepository roleRepository, ILabelService labelService, IRoleLabelRepository roleLabelRepository, IRoleEventPublisher roleEventPublisher)
    {
        _logger = logger;
        _roleRepository = roleRepository;
        _labelService = labelService;
        _roleLabelRepository = roleLabelRepository;
        _roleEventPublisher = roleEventPublisher;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var subEvent = JsonConvert.DeserializeObject<SubscriptionValidatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, accountId: {AccountId}",
        subEvent?.EventType,
        subEvent?.Data?.AccountId);

        if (subEvent?.Data == null || subEvent.Data.AccountId <= 0 || subEvent.Data.CollaboratorFunctions?.Any() != true)
        {
            return;
        }

        var accountId = subEvent.Data.AccountId;
        var collaboratorFunctions = subEvent.Data.CollaboratorFunctions;

        await CreateCollaboratorsRole(accountId, collaboratorFunctions);
    }

    private async Task CreateCollaboratorsRole(int accountId, IEnumerable<SubscriptionCollaboratorData> collaboratorFunctions)
    {
        var labels = (await _labelService.GetLabelsAsync(new Pagination())).Items!;

        foreach (var collabFunction in collaboratorFunctions)
        {
            var contactId = collabFunction.ContactId;
            var role = await _roleRepository.GetContactRoleAsync(accountId, contactId);
            var actionLevel = collabFunction.FunctionNames?.Any() == true ? (int)ActionLevelType.Contributor : (int)ActionLevelType.Observator;

            if (role != null)
            {
                var isCustomerRelation = role.IsCustomerRelation.HasValue ? role.IsCustomerRelation.Value : false;
                await _roleRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, isCustomerRelation, actionLevel);
                _logger.LogInformation($"Le rôle AccountId {accountId}/ContactId {contactId} a été mis à jour.");
            }
            else
            {
                var request = new CreateRoleRequest
                {
                    AccountId = accountId,
                    ContactId = contactId,
                    IsCustomerRelation = false,
                    ActionLevel = actionLevel,
                };

                await _roleRepository.CreateRoleAsync(request);
                _logger.LogInformation($"Le rôle AccountId {accountId}/ContactId {contactId} a été créé.");

                await _roleEventPublisher.PublishRoleCreatedEventAsync(request);
            }

            await CreateRoleLabels(labels, collabFunction, accountId, contactId);
        }
    }

    private async Task CreateRoleLabels(IEnumerable<Label> labels, SubscriptionCollaboratorData collabFunction, int accountId, int contactId)
    {
        if (collabFunction.FunctionNames?.Any() == true)
        {
            foreach (var funcName in collabFunction.FunctionNames)
            {
                var labelId = labels.Where(l => l.CollaboratorLabel.Equals(funcName)).Select(l => l.LabelId).FirstOrDefault();

                if (await _roleLabelRepository.HasRoleLabel(contactId, accountId, labelId))
                {
                    _logger.LogInformation($"Le contact {contactId} a déjà le libellé {funcName} sur l'entité {accountId}.");
                }
                else
                {
                    await _roleLabelRepository.AddRoleLabelAsync(new RoleLabel
                    {
                        AccountId = accountId,
                        ContactId = contactId,
                        LabelId = labelId,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = contactId,
                    });

                    _logger.LogInformation($"Le libellé {funcName} a été ajouté sur le rôle AccountId {accountId}/ContactId {contactId}.");
                }
            }
        }
    }
}
