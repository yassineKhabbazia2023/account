// <copyright file="ContactCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactCreatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldCreatesContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactCreatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleEventRepositoryMock = new Mock<IRoleEventRepository>();
        var roleEventPublisherMock = new Mock<IRoleEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactCreatedEventHandler(loggerMock.Object, repositoryMock.Object, roleEventRepositoryMock.Object, roleEventPublisherMock.Object);
        var message = "{\"EventType\":\"ContactCreatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactCreatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleEventRepositoryMock = new Mock<IRoleEventRepository>();
        var roleEventPublisherMock = new Mock<IRoleEventPublisher>();
        var handler = new ContactCreatedEventHandler(loggerMock.Object, repositoryMock.Object, roleEventRepositoryMock.Object, roleEventPublisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotCreateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactCreatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleEventRepositoryMock = new Mock<IRoleEventRepository>();
        var roleEventPublisherMock = new Mock<IRoleEventPublisher>();
        var handler = new ContactCreatedEventHandler(loggerMock.Object, repositoryMock.Object, roleEventRepositoryMock.Object, roleEventPublisherMock.Object);
        var message = "{\"EventType\":\"ContactCreatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactCreatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleEventRepositoryMock = new Mock<IRoleEventRepository>();
        var roleEventPublisherMock = new Mock<IRoleEventPublisher>();
        var handler = new ContactCreatedEventHandler(loggerMock.Object, repositoryMock.Object, roleEventRepositoryMock.Object, roleEventPublisherMock.Object);
        var message = "{\"EventType\":\"ContactCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_And_AccountNumber_ShouldCreateContactAndRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactCreatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleEventRepositoryMock = new Mock<IRoleEventRepository>(MockBehavior.Strict);
        var roleEventPublisherMock = new Mock<IRoleEventPublisher>(MockBehavior.Strict);

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        roleEventRepositoryMock.Setup(r => r.CreateRoleForNewContact(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new CreateRoleRequest
            {
                AccountId = 1,
                ContactId = 123,
            })
            .Verifiable();
        roleEventPublisherMock.Setup(r => r.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()))
            .Verifiable();

        var handler = new ContactCreatedEventHandler(loggerMock.Object, repositoryMock.Object, roleEventRepositoryMock.Object, roleEventPublisherMock.Object);
        var message = "{\"EventType\":\"ContactCreatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\",\"AccountNumber\":\"69696969\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Once);
        roleEventRepositoryMock.Verify();
        roleEventPublisherMock.Verify();
    }
}
