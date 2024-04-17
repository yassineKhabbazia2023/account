// <copyright file="ContactRevokedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactRevokedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRevokContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRevokedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactRevokedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RevokeContactAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRevokContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRevokedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactRevokedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.RevokeContactAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotRevokContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRevokedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactRevokedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RevokeContactAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRevokContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRevokedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactRevokedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.RevokeContactAsync(It.IsAny<int>()), Times.Never);
    }
}
