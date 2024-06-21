// <copyright file="RegistryRoleCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Core.Requests;

namespace Pulse.Role.Infrastructure.Tests.Providers;

public class RegistryRoleCreatedEventHandlerTests
{
    private readonly RoleEntity _roleEntity = new RoleEntity
    {
        AccountId = 123,
        ContactId = 456,
        IsFavorite = true,
        IsSignatory = true,
    };

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldCreatesRole_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>()))
        .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + Guid.NewGuid().ToString() + "\",\"ContactId\": \"" + Guid.NewGuid().ToString() + "\",\"Email\":\"test@email.fr\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>())) !
        .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateRole_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>())) !
        .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }
}
