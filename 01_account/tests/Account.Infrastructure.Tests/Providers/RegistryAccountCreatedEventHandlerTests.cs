// <copyright file="RegistryAccountCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RegistryAccountCreatedEventHandlerTests
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
    public async Task HandleAsync_WithValidMessage_ShouldCreatesAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        repositoryMock.Setup(r => r.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());
        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountCreatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + Guid.NewGuid().ToString() + "\",\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>()), Times.Once);
        publisherMock.Verify(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotCreateAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountCreatedEvent\",\"Data\":{\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithProspectMessage_ShouldCreateAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IAccountEventPublisher>(MockBehavior.Strict);
        var accountGuid = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DoesAccountExistAsync(accountGuid))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(r => r.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>()))
            .ReturnsAsync(_accountEntity.MapToAccountDetail());
        publisherMock
            .Setup(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()))
            .Returns(Task.CompletedTask);

        var handler = new RegistryAccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountCreatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\":\"" + accountGuid + "\",\"AccountType\":\"" + GlobalConstants.ProspectAccountType + "\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.DoesAccountExistAsync(accountGuid), Times.Once);
        repositoryMock.Verify(r => r.CreateAccountAsync(It.Is<RegistryAccountCreatedEventData>(e => e.AccountType == GlobalConstants.ProspectAccountType)), Times.Once);
        publisherMock.Verify(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingAccount_ShouldNotCreateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IAccountEventPublisher>(MockBehavior.Strict);
        var accountGuid = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.DoesAccountExistAsync(accountGuid))
            .ReturnsAsync(true);

        var handler = new RegistryAccountCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountCreatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\":\"" + accountGuid + "\",\"AccountType\":\"" + GlobalConstants.ProspectAccountType + "\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.DoesAccountExistAsync(accountGuid), Times.Once);
        repositoryMock.Verify(r => r.CreateAccountAsync(It.IsAny<RegistryAccountCreatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }
}
