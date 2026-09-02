// <copyright file="DelegationRequestEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class DelegationRequestEventPublisherTests
{
    private static readonly int[] ExpectedRecipientContactIds = { 10, 11, 12 };

    [Fact]
    public async Task PublishDelegationRequestValidatedEventAsync_ShouldPublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var delegationRequestEventPublisher = new DelegationRequestEventPublisher(publisherMock.Object);
        var validatorContactId = 10;
        var delegationRequests = new List<DelegationRequest>
        {
            new() { DelegationRequestId = 1, RequesterId = 5, RecipientId = 10, AccountId = 100, Account = new Core.Models.Account { AccountId = 100, AccountNumber = "ACC100", AccountType = "REGULAR" } },
            new() { DelegationRequestId = 2, RequesterId = 5, RecipientId = 11, AccountId = 100, Account = new Core.Models.Account { AccountId = 100, AccountNumber = "ACC100", AccountType = "REGULAR" } },
            new() { DelegationRequestId = 3, RequesterId = 5, RecipientId = 12, AccountId = 100, Account = new Core.Models.Account { AccountId = 100, AccountNumber = "ACC100", AccountType = "REGULAR" } }
        };
        BaseEvent<DelegationRequestValidatedEventData>? publishedEvent = null;
        publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestValidatedEventData>>(), null!, null))
            .Callback<BaseEvent<DelegationRequestValidatedEventData>, string, string>((@event, _, _) => publishedEvent = @event);

        // Act
        await delegationRequestEventPublisher.PublishDelegationRequestValidatedEventAsync(validatorContactId, delegationRequests);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestValidatedEventData>>(), null!, null), Times.Once);
        Assert.NotNull(publishedEvent);
        Assert.Equal("REGULAR", publishedEvent.AccountType);
        Assert.Equal(100, publishedEvent.Data.AccountId);
        Assert.Equal("ACC100", publishedEvent.Data.AccountNumber);
        Assert.Equal(5, publishedEvent.Data.RequesterContactId);
        Assert.Equal(validatorContactId, publishedEvent.Data.ValidatorContactId);
        Assert.Equal(ExpectedRecipientContactIds, publishedEvent.Data.RecipientContactIds);
    }

    [Fact]
    public async Task PublishDelegationRequestValidatedEventAsync_WhenRequestsEmpty_ShouldNotPublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var delegationRequestEventPublisher = new DelegationRequestEventPublisher(publisherMock.Object);

        // Act
        await delegationRequestEventPublisher.PublishDelegationRequestValidatedEventAsync(10, new List<DelegationRequest>());

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestValidatedEventData>>(), null!, null), Times.Never);
    }
}
