// <copyright file="ProspectConversionService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class ProspectConversionService : IProspectConversionService
{
    private readonly ILogger<ProspectConversionService> _logger;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IRegistryAccountEventRepository _accountEventRepository;
    private readonly IAccountEventPublisher _accountEventPublisher;

    public ProspectConversionService(
        ILogger<ProspectConversionService> logger,
        IFeatureFlagService featureFlagService,
        IRegistryAccountEventRepository accountEventRepository,
        IAccountEventPublisher accountEventPublisher)
    {
        _logger = logger;
        _featureFlagService = featureFlagService;
        _accountEventRepository = accountEventRepository;
        _accountEventPublisher = accountEventPublisher;
    }

    public async Task HandleClientCreatedAsync(RegistryAccountStateEventData clientEvent)
    {
        if (!await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.ProspectToClientConversion))
        {
            return;
        }

        if (clientEvent is null || !IsClient(clientEvent.AccountType))
        {
            return;
        }

        var siret = clientEvent.AccountRegisterIdentification1;
        if (string.IsNullOrWhiteSpace(siret))
        {
            return;
        }

        var prospects = await _accountEventRepository.FindActiveProspectsBySiretAsync(siret);

        if (prospects.Count == 0)
        {
            return;
        }

        if (prospects.Count > 1)
        {
            _logger.LogError(
                "Bascule prospect→client annulée: plusieurs prospects actifs partagent le SIRET {Siret}. GUIDs concernés: {Guids}",
                siret,
                string.Join(", ", prospects.Select(p => p.AccountGlobalUniqueId)));
            return;
        }

        var prospect = prospects[0];

        // Soft-delete du compte uniquement ; les rôles sont laissés intacts, comme dans le flux de suppression standard
        // (RegistryAccountRemovedEventHandler). On cible l'AccountId interne (fiable) plutôt que le GUID.
        var (removedAccountId, accountType) = await _accountEventRepository.RemoveAccountAsync(prospect.AccountId);
        await _accountEventPublisher.PublishAccountRemovedEventAsync(removedAccountId, accountType);

        _logger.LogInformation(
            "Bascule prospect→client: le prospect {AccountId} (SIRET {Siret}) a été désactivé suite à la création du client.",
            removedAccountId,
            siret);
    }

    private static bool IsClient(string? accountType) =>
        string.Equals(accountType, AccountType.CLIENT.ToString(), StringComparison.OrdinalIgnoreCase);
}
