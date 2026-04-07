// <copyright file="RegistryRoleCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;

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

    private readonly Mock<ILabelService> _labelServiceMock = new();
    private readonly Mock<IRoleLabelService> _roleLabelServiceMock = new();

    public RegistryRoleCreatedEventHandlerTests()
    {
        _labelServiceMock.Setup(s => s.GetLabelsAsync(It.IsAny<Pagination>()))
            .ReturnsAsync(new Paging<Label>
            {
                Items = new List<Label>
                {
                    new Label { LabelId = 1, Code = "CLP", CustomerLabel = "Responsable", CollaboratorLabel = "Maitre dossier", Business = "Transverse", IsVisible = false },
                    new Label { LabelId = 2, Code = "AM", CustomerLabel = "Chargé de mission", CollaboratorLabel = "Resp compte", Business = "Transverse", IsVisible = false },
                }
            });

        _roleLabelServiceMock.Setup(r => r.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);
        _roleLabelServiceMock.Setup(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()))
            .Returns(Task.CompletedTask);
        _roleLabelServiceMock.Setup(r => r.RevokeExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private RegistryRoleCreatedEventHandler CreateHandler(
        Mock<ILogger<RegistryRoleCreatedEventHandler>>? loggerMock = null,
        Mock<IRegistryRoleEventRepository>? repositoryMock = null,
        Mock<IRoleEventPublisher>? publisherMock = null,
        Mock<IHistoryEventPublisher>? historyPublisherMock = null,
        Mock<IRoleRepository>? roleRepo = null)
    {
        return new RegistryRoleCreatedEventHandler(
            (loggerMock ?? new Mock<ILogger<RegistryRoleCreatedEventHandler>>()).Object,
            (repositoryMock ?? new Mock<IRegistryRoleEventRepository>()).Object,
            (publisherMock ?? new Mock<IRoleEventPublisher>()).Object,
            (historyPublisherMock ?? new Mock<IHistoryEventPublisher>()).Object,
            (roleRepo ?? new Mock<IRoleRepository>()).Object,
            _labelServiceMock.Object,
            _roleLabelServiceMock.Object);
    }

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

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, roleRepo.Object, _labelServiceMock.Object, _roleLabelServiceMock.Object);
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

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, null!, _labelServiceMock.Object, _roleLabelServiceMock.Object);

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

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, null!, _labelServiceMock.Object, _roleLabelServiceMock.Object);
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>()), Times.Never);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync("approver@email.fr", 1, 1), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldNotCreateRole_And_PublishEvent()
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

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, roleRepo.Object, _labelServiceMock.Object, _roleLabelServiceMock.Object);

        // Act
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 2 + "\",\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\": true}}";
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()), Times.Never);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>()), Times.Once);
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
        var updatedRole = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = true,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.SetupSequence(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(role)
            .ReturnsAsync(updatedRole);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object, historyPublisherMock.Object, roleRepo.Object, _labelServiceMock.Object, _roleLabelServiceMock.Object);

        // Act
        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\": \"" + 1 + "\",\"ContactId\": \"" + 2 + "\",\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\": true}}";
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateRoleContactFlagPortailFacturesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()), Times.Once);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(
            It.Is<CreateRoleRequest>(r => r.ContactFlagPortailFactures == true), It.IsAny<string>()), Times.Once);
        historyPublisherMock.Verify(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithCLPDescription_ShouldCreateRoleLabel()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var roleRepo = new Mock<IRoleRepository>();
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":1,\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\":false,\"Description\":\"CLP\",\"IsCustomerRelation\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(
            It.Is<RoleLabel>(rl => rl.LabelId == 1 && rl.AccountId == 1 && rl.ContactId == 1)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithAMDescription_ShouldCreateRoleLabel()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var roleRepo = new Mock<IRoleRepository>();
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":1,\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\":false,\"Description\":\"AM\",\"IsCustomerRelation\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(
            It.Is<RoleLabel>(rl => rl.LabelId == 2 && rl.AccountId == 1 && rl.ContactId == 1)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoDescription_ShouldNotCreateRoleLabel()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var roleRepo = new Mock<IRoleRepository>();
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":1,\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\":false}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRoleAndCLPDescription_ShouldAssignRoleLabel()
    {
        // Arrange
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

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":2,\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\":true,\"Description\":\"CLP\",\"IsCustomerRelation\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(
            It.Is<RoleLabel>(rl => rl.LabelId == 1 && rl.AccountId == 1 && rl.ContactId == 2)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownDescription_ShouldNotCreateRoleLabel()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var roleRepo = new Mock<IRoleRepository>();
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":1,\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\":false,\"Description\":\"UNKNOWN_CODE\",\"IsCustomerRelation\":false}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        _roleLabelServiceMock.Verify(r => r.RevokeExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithCLPDescription_WhenLabelAlreadyAssignedToSameContact_ShouldNotReassign()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var roleRepo = new Mock<IRoleRepository>();
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleLabelServiceMock = new Mock<IRoleLabelService>();
        roleLabelServiceMock.Setup(r => r.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var handler = new RegistryRoleCreatedEventHandler(
            new Mock<ILogger<RegistryRoleCreatedEventHandler>>().Object,
            repositoryMock.Object, publisherMock.Object,
            historyPublisherMock.Object, roleRepo.Object, _labelServiceMock.Object, roleLabelServiceMock.Object);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":1,\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\":false,\"Description\":\"CLP\",\"IsCustomerRelation\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        roleLabelServiceMock.Verify(r => r.HasRoleLabel(1, 1, 1), Times.Once);
        roleLabelServiceMock.Verify(r => r.RevokeExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExistingRole_IsSignatoryChanged_Should_CallUpdateRoleIsSignatoryAsync()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.UpdateRoleIsSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()))
            .ReturnsAsync(true);
        var publisherMock = new Mock<IRoleEventPublisher>();

        var role = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = true,
            IsSignatory = false,
        };
        var updatedRole = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = true,
            IsSignatory = true,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.SetupSequence(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(role)
            .ReturnsAsync(updatedRole);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":2,\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\":true,\"RoleSignatory\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateRoleIsSignatoryAsync(1, 2, true), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExistingRole_IsSignatoryChanged_Should_PublishEventWithUpdatedIsSignatory()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.UpdateRoleIsSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()))
            .ReturnsAsync(true);
        var publisherMock = new Mock<IRoleEventPublisher>();

        // Simuler le comportement EF Core : la même instance est retournée par le DbContext (cache de premier niveau).
        // Sans la mise à jour manuelle de existingRole.IsSignatory, la valeur publiée resterait à false.
        var role = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = true,
            IsSignatory = false,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(role);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":2,\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\":true,\"RoleSignatory\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(
            It.Is<CreateRoleRequest>(r => r.IsSignatory == true), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExistingRole_IsSignatorySame_Should_NotCallUpdate()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        var publisherMock = new Mock<IRoleEventPublisher>();

        var role = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = true,
            IsSignatory = true,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(role);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":2,\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\":true,\"RoleSignatory\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateRoleIsSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExistingRole_UpdateIsSignatoryFails_Should_ReturnEarly()
    {
        // Arrange
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.UpdateRoleIsSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>()))
            .ReturnsAsync(false);
        var publisherMock = new Mock<IRoleEventPublisher>();

        var role = new Account.Core.Models.Role
        {
            AccountId = 1,
            ContactId = 2,
            ContactFlagPortailFactures = true,
            IsSignatory = false,
        };
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(role);

        var historyPublisherMock = new Mock<IHistoryEventPublisher>();

        var handler = CreateHandler(repositoryMock: repositoryMock, publisherMock: publisherMock, roleRepo: roleRepo, historyPublisherMock: historyPublisherMock);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":2,\"Email\":\"test@email.fr\",\"ContactFlagPortailFactures\":true,\"RoleSignatory\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateRoleIsSignatoryAsync(1, 2, true), Times.Once);
        publisherMock.Verify(p => p.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithCLPAlreadyAssignedOnAccount_ShouldReplaceRoleLabel()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryRoleCreatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryRoleEventRepository>();
        repositoryMock.Setup(r => r.CreateRoleAsync(It.IsAny<RegistryRoleCreatedEventData>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool?>()))
            .ReturnsAsync(_roleEntity.ToCreateRoleRequest());
        var publisherMock = new Mock<IRoleEventPublisher>();
        var roleRepo = new Mock<IRoleRepository>();
        var historyPublisherMock = new Mock<IHistoryEventPublisher>();
        historyPublisherMock.Setup(h => h.PublishHistoryCreatedEventAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleLabelServiceMock = new Mock<IRoleLabelService>();
        roleLabelServiceMock.Setup(r => r.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);
        roleLabelServiceMock.Setup(r => r.RevokeExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        roleLabelServiceMock.Setup(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()))
            .Returns(Task.CompletedTask);

        var handler = new RegistryRoleCreatedEventHandler(
            loggerMock.Object, repositoryMock.Object, publisherMock.Object,
            historyPublisherMock.Object, roleRepo.Object, _labelServiceMock.Object, roleLabelServiceMock.Object);

        var message = "{\"EventType\":\"RegistryRoleCreatedEvent\",\"Data\":{\"AccountId\":1,\"ContactId\":1,\"Email\":\"test@email.fr\",\"RegistryApproverEmail\":\"approver@email.fr\",\"ContactFlagPortailFactures\":false,\"Description\":\"CLP\",\"IsCustomerRelation\":true}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        roleLabelServiceMock.Verify(r => r.RevokeExclusiveLabelAsync(1, 1, "CLP"), Times.Once);
        roleLabelServiceMock.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Once);
    }
}
