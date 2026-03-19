// <copyright file="OfferActivatedEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class OfferActivatedEventPublisher : IOfferActivatedEventPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IAccountRepository _accountRepository;

    public OfferActivatedEventPublisher(IEventPublisher eventPublisher, IAccountRepository accountRepository)
    {
        _eventPublisher = eventPublisher;
        _accountRepository = accountRepository;
    }

    public async Task PublishOfferActivatedEventAsync(int accountId, string offerName)
    {
        var account = await _accountRepository.GetAccountAsync(accountId);

        var data = new OfferActivatedEventData
        {
            AccountId = accountId,
            OfferName = offerName
        };

        await _eventPublisher.PublishAsync(new OfferActivatedEvent(data) { AccountType = account.AccountType });
    }
}
