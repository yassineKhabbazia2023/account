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
        ContactFlagPortailFactures = false,
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

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, roleRepo.Object);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 1 + "\",\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\": false,\"SubRole\":\"executive\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), "executive"), Times.Once);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync("approver@email.fr", 1, 1), Times.Once);
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

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, null!);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>()), Times.Never);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync("approver@email.fr", 1, 1), Times.Never);
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
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, null!);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>()), Times.Never);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync("approver@email.fr", 1, 1), Times.Never);
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
            ContactId = 2,
            ContactFlagPortailFactures = true,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(role);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, roleRepo.Object);

        // Act
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 2 + "\",\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\": true}}";
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>()), Times.Never);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync("approver@email.fr", 1, 1), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRoleAndDifferentContactFlag_ShouldUpdateRole_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.UpdateRoleContactFlagPortailFacturesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()))
            .ReturnsAsync((int accountId, int contactId, bool? contactFlagPortailFactures) => new CreateRoleRequest
            {
                AccountId = accountId,
                ContactId = contactId,
                ContactFlagPortailFactures = contactFlagPortailFactures
            });
        var publisherMock = new Mock<IRoleEventPublisher>();

        var role = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = false,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(role);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, roleRepo.Object);

        // Act
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 2 + "\",\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\": true}}";
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateRoleContactFlagPortailFacturesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(
            It.Is<CreateRoleRequest>(r => r.ContactFlagPortailFactures == true), It.IsAny<string>()), Times.Once);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
