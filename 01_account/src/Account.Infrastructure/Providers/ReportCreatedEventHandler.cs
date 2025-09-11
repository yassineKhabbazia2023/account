// <copyright file="ReportCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Providers;

public class ReportCreatedEventHandler : IEventHandler
{
    private readonly ILogger<ReportCreatedEventHandler> _logger;
    private readonly IAccountEventRepository _accountEventRepository;
    private readonly IOfferEligibilityEventRepository _offerEligibilityEventRepository;
    private const int CLARITY_TYPE_ID = 9;
    private const string CLARITY_OFFER = "Clarity";

    public ReportCreatedEventHandler(ILogger<ReportCreatedEventHandler> logger,
       IAccountEventRepository accountEventRepository,
       IOfferEligibilityEventRepository offerEligibilityEventRepository)
    {
        _logger = logger;
        _accountEventRepository = accountEventRepository;
        _offerEligibilityEventRepository = offerEligibilityEventRepository;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var reportEvent = JsonConvert.DeserializeObject<ReportCreatedEvent>(message);

        if (reportEvent?.Data == null || reportEvent.Data.ReportId <= 0 || reportEvent.Data.AccountId < -1)
        {
            return;
        }

        _logger.LogInformation("Consommation de l'event n°{EventId} type: {EventType}, reportId: {ReportId}, accountId: {AccountId}",
                reportEvent.EventId,
                reportEvent.EventType,
                reportEvent.Data?.ReportId,
                reportEvent.Data?.AccountId);

        var report = reportEvent.Data!;

        if (await _accountEventRepository.DoesAccountExistAsync(report.AccountId)
            && !await _offerEligibilityEventRepository.DoesOfferEligibilityExistsAsync(report.AccountId)
            && report.ReportTypeId == CLARITY_TYPE_ID)
        {
            var offerEligibility = new OfferEligibilityEntity
            {
                AccountId = report.AccountId,
                OfferName = CLARITY_OFFER,
                IsEligible = true,
                ReportId = report.ReportId,
                ReportLabel = report.ReportLabel,
            };

            await _offerEligibilityEventRepository.CreateOfferEligibilityAsync(offerEligibility);

            _logger.LogInformation("OfferEligibility crée pour l'entité {AccountId}", report.AccountId);
        }
    }
}
