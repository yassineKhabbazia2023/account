// <copyright file="DelegationRequestEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class DelegationRequestEventPublisher(IEventPublisher eventPublisher) : IDelegationRequestEventPublisher
{
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public async Task PublishDelegationRequestCreatedEventAsync(AccountDetail account, int requesterId, IEnumerable<int> recipientIds)
    {
        var data = new DelegationRequestCreatedEventData
        {
            AccountId = account.AccountId,
            RequesterId = requesterId,
            RecipientIds = recipientIds,
        };

        await _eventPublisher.PublishAsync(new DelegationRequestCreatedEvent(data) { AccountType = account.AccountType });
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

    public async Task PublishDelegationRequestRefusedEventAsync(int refuserContactId, int requesterContactId, int accountId, string? accountType)
    {
        var eventData = new DelegationRequestRefusedEventData
        {
            AccountId = accountId,
            RequesterContactId = requesterContactId,
            RefuserContactId = refuserContactId,
        };

        await _eventPublisher.PublishAsync(new DelegationRequestRefusedEvent(eventData) { AccountType = accountType });
    }
}
