// <copyright file="ContactUpdatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Core.Enum;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactUpdatedEventHandler : IEventHandler
{
    private readonly ILogger<ContactUpdatedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;
    private readonly IAccountEventRepository _accountEventRepository;

    public ContactUpdatedEventHandler(
        ILogger<ContactUpdatedEventHandler> logger,
        IContactEventRepository contactEventRepository,
        IAccountEventRepository accountEventRepository)
    {
        _logger = logger;
        _contactEventRepository = contactEventRepository;
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

        var contactEntity = contactEvent!.Data.ToContactEntity();

        await _contactEventRepository.UpdateContactAsync(contactEntity!);

        var accountEntity = _accountEventRepository.GetAccountBySignatory(contactEntity!.ContactId);

        if (accountEntity != null && accountEntity.Count != 0)
        {
            var contactStatus = _contactEventRepository.GetContactById(contactEntity.ContactId)!.Status;
            int deploymentStatus = 0;

            if (contactStatus.Equals(ContactStatus.Invited.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                deploymentStatus = (int)DeploymentStatus.InProgress;
            }
            else if (contactStatus.Equals(ContactStatus.Connected.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                deploymentStatus = (int)DeploymentStatus.Connected;
            }
            else
            {
                deploymentStatus = (int)DeploymentStatus.ToDeploy;
            }

            var accountIds = await _accountEventRepository.UpdateAccountStatusByContactAsync(accountEntity.Select(x => x.AccountId), deploymentStatus);

            _logger.LogInformation("L'entité avec l'identifiant: {AccountId} vient d'être modifié.", string.Join('-', accountIds));
        }

        _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être modifié.", contactEntity.ContactId);
    }
}
