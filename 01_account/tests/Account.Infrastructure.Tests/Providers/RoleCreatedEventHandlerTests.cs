// <copyright file="RoleCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RoleCreatedEventHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<RoleCreatedEventHandler>> _logger;

    public RoleCreatedEventHandlerTests()
    {
        _fixture = new Fixture();
        _logger = new Mock<ILogger<RoleCreatedEventHandler>>();
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishEvent()
    {
        var roles = _fixture.CreateMany<CreateRoleRequest>(3);
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123, \"AccountId\":456}}";
        var eventRepository = new Mock<IRoleEventRepository>();
        eventRepository.Setup(r => r.CreateRoleForAutomaticDelegationsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(roles);
        var publisher = new Mock<IRoleEventPublisher>();
        var historyPublisher = new Mock<IHistoryEventPublisher>();

        var handler = new RoleCreatedEventHandler(_logger.Object, eventRepository.Object, publisher.Object, historyPublisher.Object);
        await handler.HandleAsync(message);

        publisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_WhenRolesCreatedForAutomaticDelegations_ShouldPublishHistoryWithAddkdela()
    {
        var delegatorContactId = 123;
        var delegateeContactIds = new[] { 11, 22, 33 };
        var accountId = 456;
        var roles = delegateeContactIds.Select(id => new CreateRoleRequest { AccountId = accountId, ContactId = id }).ToList();
        var message = $"{{\"EventType\":\"RoleCreatedEvent\",\"Data\":{{\"ContactId\":{delegatorContactId}, \"AccountId\":{accountId}}}}}";

        var eventRepository = new Mock<IRoleEventRepository>();
        eventRepository.Setup(r => r.CreateRoleForAutomaticDelegationsAsync(delegatorContactId, accountId)).ReturnsAsync(roles);
        var publisher = new Mock<IRoleEventPublisher>();
        var historyPublisher = new Mock<IHistoryEventPublisher>();

        var handler = new RoleCreatedEventHandler(_logger.Object, eventRepository.Object, publisher.Object, historyPublisher.Object);
        await handler.HandleAsync(message);

        historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(delegatorContactId, It.IsAny<int>(), accountId, ActionCode.ADDKDELA.ToString()),
            Times.Exactly(3));

        foreach (var delegateeId in delegateeContactIds)
        {
            historyPublisher.Verify(
                x => x.PublishHistoryCreatedEventAsync(delegatorContactId, delegateeId, accountId, ActionCode.ADDKDELA.ToString()),
                Times.Once);
        }
    }

    [Fact]
    public async Task HandleAsync_WhenNoRoleCreated_ShouldNotPublishHistory()
    {
        var message = "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123, \"AccountId\":456}}";
        var eventRepository = new Mock<IRoleEventRepository>();
        eventRepository.Setup(r => r.CreateRoleForAutomaticDelegationsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<CreateRoleRequest>());
        var publisher = new Mock<IRoleEventPublisher>();
        var historyPublisher = new Mock<IHistoryEventPublisher>();

        var handler = new RoleCreatedEventHandler(_logger.Object, eventRepository.Object, publisher.Object, historyPublisher.Object);
        await handler.HandleAsync(message);

        historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [MemberData(nameof(Messages))]
    public async Task HandleAsync_WithNullOrEmptyMessage_Should_Return(string message)
    {
        var publisher = new Mock<IRoleEventPublisher>();
        var historyPublisher = new Mock<IHistoryEventPublisher>();
        var handler = new RoleCreatedEventHandler(_logger.Object, null!, publisher.Object, historyPublisher.Object);
        await handler.HandleAsync(message);

        publisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        historyPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    public static TheoryData<string> Messages => new()
    {
        null!,
        string.Empty,
        "{\"EventType\":\"RoleCreatedEvent\"}",
        "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":0, \"AccountId\":456}}",
        "{\"EventType\":\"RoleCreatedEvent\",\"Data\":{\"ContactId\":123, \"AccountId\":-2}}"
    };
}
