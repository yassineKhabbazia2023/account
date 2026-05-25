// <copyright file="SubscriptionValidatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Providers;
using RoleModel = Pulse.Account.Core.Models.Role;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class SubscriptionValidatedEventHandlerTests
{
    private readonly Mock<ILogger<SubscriptionValidatedEventHandler>> _logger;
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Mock<IRoleLabelRepository> _roleLabelRepository;
    private readonly Mock<ILabelService> _labelService;
    private readonly Mock<IRoleEventPublisher> _roleEventPublisher;

    public SubscriptionValidatedEventHandlerTests()
    {
        _logger = new Mock<ILogger<SubscriptionValidatedEventHandler>>();
        _roleRepository = new Mock<IRoleRepository>();
        _roleLabelRepository = new Mock<IRoleLabelRepository>();
        _labelService = new Mock<ILabelService>();
        _roleEventPublisher = new Mock<IRoleEventPublisher>();
    }

    [Fact]
    public async Task HandleAsync_WithNoExistingRoleOrRoleLabel_ShouldCreateRoleAndRoleLabel()
    {
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":1,\"CollaboratorFunctions\": [{\"ContactId\":1,\"FunctionNames\":[] }, {\"ContactId\":2,\"FunctionNames\":[\"func1\", \"func2\"] }]}}";
        _roleLabelRepository.Setup(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
        _labelService.Setup(x => x.GetLabelsAsync(It.IsAny<Pagination>())).ReturnsAsync(GetLabels());

        var handler = new SubscriptionValidatedEventHandler(_logger.Object, _roleRepository.Object, _labelService.Object, _roleLabelRepository.Object, _roleEventPublisher.Object);

        await handler.HandleAsync(message);

        _roleRepository.Verify(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        _roleRepository.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>()), Times.Exactly(2));
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        _roleLabelRepository.Verify(x => x.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Exactly(2));
        _labelService.Verify(x => x.GetLabelsAsync(It.IsAny<Pagination>()), Times.Once);
        _roleEventPublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WithExistingRoleAndRoleLabel_ShouldCreateRoleAndRoleLabel()
    {
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":1,\"CollaboratorFunctions\": [{\"ContactId\":1,\"FunctionNames\":[] }, {\"ContactId\":2,\"FunctionNames\":[\"func1\", \"func2\"] }]}}";
        var role = new RoleModel { IsCustomerRelation = true };
        _roleRepository.Setup(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(role);
        _roleRepository.Setup(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _roleEventPublisher.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _roleLabelRepository.Setup(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        _labelService.Setup(x => x.GetLabelsAsync(It.IsAny<Pagination>())).ReturnsAsync(GetLabels());

        var handler = new SubscriptionValidatedEventHandler(_logger.Object, _roleRepository.Object, _labelService.Object, _roleLabelRepository.Object, _roleEventPublisher.Object);

        await handler.HandleAsync(message);

        _roleRepository.Verify(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        _roleRepository.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Exactly(2));
        _roleLabelRepository.Verify(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        _roleLabelRepository.Verify(x => x.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        _labelService.Verify(x => x.GetLabelsAsync(It.IsAny<Pagination>()), Times.Once);
        _roleEventPublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _roleEventPublisher.Verify(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateRoleAndRoleLabel()
    {
        var handler = new SubscriptionValidatedEventHandler(null!, _roleRepository.Object, _labelService.Object, _roleLabelRepository.Object, _roleEventPublisher.Object);

        await handler.HandleAsync(null!);

        _roleRepository.Verify(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleRepository.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        _labelService.Verify(x => x.GetLabelsAsync(It.IsAny<Pagination>()), Times.Never);
        _roleEventPublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidAccountId_ShouldNotCreateRoleAndRoleLabel()
    {
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":-10}}";
        var handler = new SubscriptionValidatedEventHandler(_logger.Object, _roleRepository.Object, _labelService.Object, _roleLabelRepository.Object, _roleEventPublisher.Object);

        await handler.HandleAsync(message);

        _roleRepository.Verify(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleRepository.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        _labelService.Verify(x => x.GetLabelsAsync(It.IsAny<Pagination>()), Times.Never);
        _roleEventPublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullCollaboratorFunctions_ShouldNotCreateRoleAndRoleLabel()
    {
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":1000}}";
        var handler = new SubscriptionValidatedEventHandler(_logger.Object, _roleRepository.Object, _labelService.Object, _roleLabelRepository.Object, _roleEventPublisher.Object);

        await handler.HandleAsync(message);

        _roleRepository.Verify(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleRepository.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        _labelService.Verify(x => x.GetLabelsAsync(It.IsAny<Pagination>()), Times.Never);
        _roleEventPublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyCollaboratorFunctions_ShouldNotCreateRoleAndRoleLabel()
    {
        var message = "{\"EventType\":\"SubscriptionValidatedEvent\",\"Data\":{\"AccountId\":1000,\"CollaboratorFunctions\": []}}";
        var handler = new SubscriptionValidatedEventHandler(_logger.Object, _roleRepository.Object, _labelService.Object, _roleLabelRepository.Object, _roleEventPublisher.Object);

        await handler.HandleAsync(message);

        _roleRepository.Verify(x => x.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleRepository.Verify(x => x.CreateRoleAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleLabelRepository.Verify(x => x.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        _labelService.Verify(x => x.GetLabelsAsync(It.IsAny<Pagination>()), Times.Never);
        _roleEventPublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    private static Paging<Label> GetLabels() => new Paging<Label>
    {
        Items = new List<Label>
            {
                new()
                {
                    LabelId = 1,
                    Code = "func1",
                    CollaboratorLabel = "func1",
                    CustomerLabel = "func1",
                    Business = "biz1",
                    IsVisible = true,
                },
                new()
                {
                    LabelId = 2,
                    Code = "func2",
                    CollaboratorLabel = "func2",
                    CustomerLabel = "func2",
                    Business = "biz1",
                    IsVisible = true,
                },
                new()
                {
                    LabelId = 3,
                    Code = "func3",
                    CollaboratorLabel = "func3",
                    CustomerLabel = "func3",
                    Business = "biz3",
                    IsVisible = true,
                }
            }
    };
}
