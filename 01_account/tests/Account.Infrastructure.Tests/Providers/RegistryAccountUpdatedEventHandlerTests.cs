// <copyright file="RegistryAccountUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RegistryAccountUpdatedEventHandlerTests
{
    private readonly AccountEntity _accountEntity = new AccountEntity
    {
        AccountId = 1,
        AccountGlobalUniqueId = Guid.NewGuid(),
        AccountNumber = "4242424242",
        LegalName = "Jooooohnnnnyyyy Piza",
        CreatedBy = "Me",
        Email = "me@me.fr",
    };

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + Guid.NewGuid().ToString() + "\",\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Once);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotUpdateAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }
}
