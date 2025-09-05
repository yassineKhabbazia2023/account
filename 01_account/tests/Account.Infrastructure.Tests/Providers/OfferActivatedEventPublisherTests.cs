// <copyright file="OfferActivatedEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class OfferActivatedEventPublisherTests
{
    private readonly Mock<IEventPublisher> _eventPublisher;

    public OfferActivatedEventPublisherTests()
    {
        _eventPublisher = new Mock<IEventPublisher>();
    }

    [Fact]
    public async Task PublishOfferActivatedEventAsync_Nominal()
    {
        var publisher = new OfferActivatedEventPublisher(_eventPublisher.Object);

        await publisher.PublishOfferActivatedEventAsync(1, "name");

        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<OfferActivatedEventData>>(), null!, null), Times.Once);
    }
}
