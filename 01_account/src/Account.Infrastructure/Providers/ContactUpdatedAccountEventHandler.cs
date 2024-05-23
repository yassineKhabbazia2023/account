// <copyright file="ContactUpdatedAccountEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Enum;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactUpdatedAccountEventHandler : IEventHandler
{
    private readonly ILogger<ContactUpdatedAccountEventHandler> _logger;
    private readonly IAccountEventRepository _accountEventRepository;

    public ContactUpdatedAccountEventHandler(
        ILogger<ContactUpdatedAccountEventHandler> logger,
        IAccountEventRepository accountEventRepository)
    {
        _logger = logger;
        _accountEventRepository = accountEventRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var contactEvent = JsonConvert.DeserializeObject<ContactUpdatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            contactEvent?.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent?.Data?.ContactId <= 0)
        {
            return;
        }

        var contactParam = contactEvent!.Data.ToContactEntity();
        var contactEntity = _accountEventRepository.GetContactById(contactParam.ContactId);

        var accountEntity = _accountEventRepository.GetAccountQueryByContactId(contactEntity!.ContactId);

        if(accountEntity != null)
        {
            var deploymentStatus = contactEntity.Status == ContactStatus.Invited.ToString().ToLower() ? DeploymentStatus.InProgress :
                                    (contactEntity.Status == ContactStatus.Connected.ToString().ToLower() ? DeploymentStatus.Connected : DeploymentStatus.ToDeploy);

            var accountIds = await _accountEventRepository.UpdateAccountStatusByContactAsync(accountEntity.Select(x => x.AccountId), (int)deploymentStatus);

            _logger.LogInformation("L'entité avec l'identifiant: {AccountId} vient d'être modifié.", string.Join('-', accountIds));
        }
        else
        {
            _logger.LogInformation("Le contact avec l'identifiant: {ContactId} n'est pas signataire.", contactEntity.ContactId);
        }
    }
}
