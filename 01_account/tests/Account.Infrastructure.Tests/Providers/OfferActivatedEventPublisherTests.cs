// <copyright file="OfferActivatedEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class OfferActivatedEventPublisherTests
{
    private readonly Mock<IEventPublisher> _eventPublisher;
    private readonly Mock<IAccountRepository> _accountRepository;

    public OfferActivatedEventPublisherTests()
    {
        _eventPublisher = new Mock<IEventPublisher>();
        _accountRepository = new Mock<IAccountRepository>();
    }

    [Fact]
    public async Task PublishOfferActivatedEventAsync_Nominal()
    {
        _accountRepository.Setup(a => a.GetAccountAsync(1)).ReturnsAsync(new AccountDetail
        {
            AccountId = 1,
            AccountNumber = "123",
            AccountType = "CLIENT",
            Legal = new Legal { LegalName = "Test" },
            Phone = new List<Phone>(),
        });

        var publisher = new OfferActivatedEventPublisher(_eventPublisher.Object, _accountRepository.Object);

        await publisher.PublishOfferActivatedEventAsync(1, "name");

        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<OfferActivatedEventData>>(), null!, null), Times.Once);
    }
}
