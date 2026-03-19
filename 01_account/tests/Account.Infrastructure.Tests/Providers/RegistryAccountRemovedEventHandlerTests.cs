// <copyright file="RegistryAccountRemovedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Constants;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RegistryAccountRemovedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRemoveAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountRemovedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + Guid.NewGuid().ToString() + "\",\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<Guid>()), Times.Once);
        publisherMock.Verify(p => p.PublishAccountRemovedEventAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRemoveAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<Guid>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountRemovedEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotRemoveAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountRemovedEvent\",\"Data\":{\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<Guid>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountRemovedEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRemoveAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountRemovedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveAccountAsync(It.IsAny<Guid>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountRemovedEventAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithProspectAccount_ShouldRemoveAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IAccountEventPublisher>(MockBehavior.Strict);
        var accountGuid = Guid.NewGuid();

        repositoryMock.Setup(r => r.RemoveAccountAsync(accountGuid))
            .ReturnsAsync(12);
        publisherMock.Setup(p => p.PublishAccountRemovedEventAsync(12))
            .Returns(Task.CompletedTask);

        var handler = new RegistryAccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountRemovedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\":\"" + accountGuid + "\",\"AccountType\":\"" + GlobalConstants.ProspectAccountType + "\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.RemoveAccountAsync(accountGuid), Times.Once);
        publisherMock.Verify(p => p.PublishAccountRemovedEventAsync(12), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonProspectAccount_ShouldRemoveAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IAccountEventPublisher>(MockBehavior.Strict);
        var accountGuid = Guid.NewGuid();

        repositoryMock.Setup(r => r.RemoveAccountAsync(accountGuid))
            .ReturnsAsync(42);
        publisherMock.Setup(p => p.PublishAccountRemovedEventAsync(42))
            .Returns(Task.CompletedTask);

        var handler = new RegistryAccountRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountRemovedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\":\"" + accountGuid + "\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.RemoveAccountAsync(accountGuid), Times.Once);
        publisherMock.Verify(p => p.PublishAccountRemovedEventAsync(42), Times.Once);
    }
}
