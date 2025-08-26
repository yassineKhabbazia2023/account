// <copyright file="AccountEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class AccountEventPublisherTests
{
    private readonly Fixture _fixture;

    public AccountEventPublisherTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task PublishAccountCreatedEventAsync_Should_PublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var accountEventPublisher = new AccountEventPublisher(publisherMock.Object);
        var detail = _fixture.Create<AccountDetail>();

        // Act
        await accountEventPublisher.PublishAccountCreatedEventAsync(detail);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<AccountStateEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishAccountUpdateddEventAsync_Should_PublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var accountEventPublisher = new AccountEventPublisher(publisherMock.Object);
        var detail = _fixture.Create<AccountDetail>();

        // Act
        await accountEventPublisher.PublishAccountUpdatedEventAsync(detail);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<AccountStateEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishAccountRemovedEventAsync_Should_PublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var accountEventPublisher = new AccountEventPublisher(publisherMock.Object);
        var detail = _fixture.Create<AccountDetail>();

        // Act
        await accountEventPublisher.PublishAccountRemovedEventAsync(It.IsAny<int>());

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<AccountRemovedEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishAccountCreatedStateAsync_ShouldDoNothingIfAccountIsNull()
    {
        var publisherMock = new Mock<IEventPublisher>();
        var accountEventPublisher = new AccountEventPublisher(publisherMock.Object);
        AccountDetail account = null;

        await accountEventPublisher.PublishAccountCreatedEventAsync(account);

        publisherMock.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<AccountStateEventData>>(), null, null), Times.Never);
    }

    [Fact]
    public async Task PublishAccountUpdatedEventAsync_ShouldDoNothing_IfAccountIsNull()
    {
        var publisherMock = new Mock<IEventPublisher>();
        var accountEventPublisher = new AccountEventPublisher(publisherMock.Object);
        AccountDetail account = null;

        await accountEventPublisher.PublishAccountUpdatedEventAsync(account);

        publisherMock.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<AccountStateEventData>>(), null, null), Times.Never);
    }
}
