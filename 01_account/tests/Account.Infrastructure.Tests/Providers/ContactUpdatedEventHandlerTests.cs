// <copyright file="ContactUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactUpdatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(repository => repository.GetContactById(123)).Returns(new ContactEntity()
        {
            ContactId = 123,
            Status = "Connected"
        }).Verifiable();
        repositoryMock.Setup(repository => repository.UpdateContactAsync(It.IsAny<ContactEntity>())).Returns(Task.CompletedTask).Verifiable();
        repositoryMock.Setup(repository => repository.GetAccountBySignatory(It.IsAny<int>())).Returns(new List<AccountEntity>()).Verifiable();
        repositoryMock.Setup(repository => repository.UpdateAccountStatusByContactAsync(new List<int>(), 3)).ReturnsAsync(new List<int>()).Verifiable();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.GetAccountBySignatory(It.IsAny<int>()), Times.Once);
        repositoryMock.Verify(repo => repo.GetContactById(123), Times.Once);
        repositoryMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(new List<int>(), 3), Times.Once);
    }
}
