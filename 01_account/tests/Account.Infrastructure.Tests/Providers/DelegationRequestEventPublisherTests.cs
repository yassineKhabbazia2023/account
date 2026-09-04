// <copyright file="DelegationRequestEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
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


    [Fact]
    public async Task PublishDelegationRequestRefusedEventAsync_ShouldPublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var delegationRequestEventPublisher = new DelegationRequestEventPublisher(publisherMock.Object);
        BaseEvent<DelegationRequestRefusedEventData>? publishedEvent = null;
        publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestRefusedEventData>>(), null!, null))
            .Callback<BaseEvent<DelegationRequestRefusedEventData>, string, string>((@event, _, _) => publishedEvent = @event);

        // Act
        await delegationRequestEventPublisher.PublishDelegationRequestRefusedEventAsync(12, 5, 100, "REGULAR");

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestRefusedEventData>>(), null!, null), Times.Once);
        Assert.NotNull(publishedEvent);
        Assert.Equal("REGULAR", publishedEvent.AccountType);
        Assert.Equal(100, publishedEvent.Data.AccountId);
        Assert.Equal(5, publishedEvent.Data.RequesterContactId);
        Assert.Equal(12, publishedEvent.Data.RefuserContactId);
    }

    [Fact]
    public async Task PublishDelegationRequestCreatedEventAsync_Should_PublishEventWithAccountData()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();

        var accountId = 1;
        var requesterId = 5;
        var recipientIds = new[] { 2, 3 };
        var accountType = "CLIENT";

        var account = new AccountDetail
        {
            AccountId = accountId,
            AccountNumber = "123",
            AccountType = accountType,
            Legal = new Legal { LegalName = "Test" },
            Phone = new List<Phone>(),
        };

        publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestCreatedEventData>>(), null!, null))
            .Callback<BaseEvent<DelegationRequestCreatedEventData>, string, string>((@event, _, _) =>
            {
                Assert.Equal(accountId, @event.Data.AccountId);
                Assert.Equal(requesterId, @event.Data.RequesterId);
                Assert.Equal(recipientIds, @event.Data.RecipientIds);
                Assert.Equal(accountType, @event.AccountType);
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        var eventPublisher = new DelegationRequestEventPublisher(publisherMock.Object);

        // Act
        await eventPublisher.PublishDelegationRequestCreatedEventAsync(account, requesterId, recipientIds);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<DelegationRequestCreatedEventData>>(), null!, null), Times.Once);
    }
}
