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
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
        .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var roleRepo = new Mock<IRoleRepository>();

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, roleRepo.Object);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 1 + "\",\"Email\":\"test@email.fr\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>())) !
        .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, null!);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateRole_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>())) !
        .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, null!);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldNotCreateRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        var publisherMock = new Mock<IRoleEventPublisher>();

        var role = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(role);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, roleRepo.Object);

        // Act
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 2 + "\",\"Email\":\"test@email.fr\"}}";
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }
}
