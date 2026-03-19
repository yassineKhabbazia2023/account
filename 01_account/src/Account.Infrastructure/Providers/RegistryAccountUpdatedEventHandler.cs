// <copyright file="RegistryAccountUpdatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class RegistryAccountUpdatedEventHandler : IEventHandler
{
    private readonly ILogger<RegistryAccountUpdatedEventHandler> _logger;
    private readonly IRegistryAccountEventRepository _accountEventRepository;
    private readonly IAccountEventPublisher _accountEventPublisher;

    public RegistryAccountUpdatedEventHandler(
        ILogger<RegistryAccountUpdatedEventHandler> logger,
        IRegistryAccountEventRepository accountEventRepository,
        IAccountEventPublisher accountEventPublisher)
    {
        _logger = logger;
        _accountEventRepository = accountEventRepository;
        _accountEventPublisher = accountEventPublisher;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var @event = JsonConvert.DeserializeObject<RegistryAccountUpdatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, Id: {Id}",
            @event?.EventType,
            @event?.Data?.AccountGlobalUniqueIdentifier);

        if (@event?.Data == null || @event?.Data.AccountGlobalUniqueIdentifier == default(Guid))
        {
            return;
        }

        // Protéger les champs obligatoires avant la mise à jour
        await ProtectRequiredFieldsAsync(@event.Data);

        var updatedAccount = await _accountEventRepository.UpdateAccountAsync(@event!.Data);
        _logger.LogInformation("L'entité avec l'identifiant suivant: {AccountId} vient d'être mise à jour.", updatedAccount.AccountId);

        await _accountEventPublisher.PublishAccountUpdatedEventAsync(updatedAccount);
    }

    private async Task ProtectRequiredFieldsAsync(Back.Events.IntegrationEvents.EventsData.RegistryAccountUpdatedEventData eventData)
    {
        var currentAccount = await _accountEventRepository.GetAccountByGuidAsync(eventData.AccountGlobalUniqueIdentifier);

        if (currentAccount == null)
        {
            return;
        }

        // Protéger StaffSizeRange
        if (!string.IsNullOrWhiteSpace(currentAccount.Legal?.StaffSizeRange)
            && string.IsNullOrWhiteSpace(eventData.AccountStaffSizeSlice))
        {
            _logger.LogError(
                "Tentative de suppression du champ StaffSizeRange pour le compte {AccountGuid} via événement Registry. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.",
                eventData.AccountGlobalUniqueIdentifier,
                currentAccount.Legal.StaffSizeRange);

            eventData.AccountStaffSizeSlice = currentAccount.Legal.StaffSizeRange;
        }

        // Protéger AccountingType
        if (!string.IsNullOrWhiteSpace(currentAccount.Accounting?.AccountingType)
            && string.IsNullOrWhiteSpace(eventData.AccountTypeTenueComptable))
        {
            _logger.LogError(
                "Tentative de suppression du champ AccountingType pour le compte {AccountGuid} via événement Registry. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.",
                eventData.AccountGlobalUniqueIdentifier,
                currentAccount.Accounting.AccountingType);

            eventData.AccountTypeTenueComptable = currentAccount.Accounting.AccountingType;
        }
    }
}
