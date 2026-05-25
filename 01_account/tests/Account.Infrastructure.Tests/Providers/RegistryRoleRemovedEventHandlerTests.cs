// <copyright file="RegistryRoleRemovedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Role.Infrastructure.Tests.Providers;

public class RegistryRoleRemovedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRemovesRole_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(x => x.RemoveRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        var publisherMock = new Mock<IRoleEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryRoleRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryRoleRemovedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 1 + "\",\"Email\":\"test@email.fr\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), true), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoRole_ShouldRemovesRole_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(x => x.RemoveRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
        var publisherMock = new Mock<IRoleEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryRoleRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryRoleRemovedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 1 + "\",\"Email\":\"test@email.fr\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRemoveRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        var publisherMock = new Mock<IRoleEventPublisher>();
        var handler = new RegistryRoleRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRemoveRole_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleRemovedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        var publisherMock = new Mock<IRoleEventPublisher>();
        var handler = new RegistryRoleRemovedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryRoleRemovedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RemoveRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }
}
