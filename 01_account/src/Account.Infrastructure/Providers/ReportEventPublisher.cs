// <copyright file="ReportEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.Infrastructure.Providers;

public class ReportEventPublisher : IReportEventPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IAccountRepository _accountRepository;

    public ReportEventPublisher(IEventPublisher eventPublisher, IAccountRepository accountRepository)
    {
        _eventPublisher = eventPublisher;
        _accountRepository = accountRepository;
    }

    public async Task PublishReportCreatedEventAsync(int? reportId, int accountId, int reportTypeId, string reportLabel, ReportStatus reportStatus)
    {
        var account = await _accountRepository.GetAccountAsync(accountId);

        var eventData = new ReportCreatedEventData
        {
            ReportId = reportId,
            AccountId = accountId,
            ReportLabel = reportLabel,
            ReportTypeId = reportTypeId,
            ReportStatus = reportStatus.ToString()
        };

        var @event = new ReportCreatedEvent(eventData) { AccountType = account.AccountType };
        await _eventPublisher.PublishAsync(@event);
    }
}
