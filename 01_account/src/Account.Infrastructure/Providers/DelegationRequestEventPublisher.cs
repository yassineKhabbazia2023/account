// <copyright file="DelegationRequestEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class DelegationRequestEventPublisher : IDelegationRequestEventPublisher
{
    private readonly IEventPublisher _eventPublisher;

    public DelegationRequestEventPublisher(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task PublishDelegationRequestValidatedEventAsync(int validatorContactId, List<DelegationRequest> delegationRequests)
    {
        if (delegationRequests is null || delegationRequests.Count == 0)
        {
            return;
        }

        var firstRequest = delegationRequests[0];

        var eventData = new DelegationRequestValidatedEventData
        {
            AccountId = firstRequest.AccountId,
            AccountNumber = firstRequest.Account?.AccountNumber,
            RequesterContactId = firstRequest.RequesterId,
            ValidatorContactId = validatorContactId,
            RecipientContactIds = delegationRequests.Select(dr => dr.RecipientId).Distinct().ToList(),
        };

        await _eventPublisher.PublishAsync(new DelegationRequestValidatedEvent(eventData) { AccountType = firstRequest.Account?.AccountType });
    }
}
