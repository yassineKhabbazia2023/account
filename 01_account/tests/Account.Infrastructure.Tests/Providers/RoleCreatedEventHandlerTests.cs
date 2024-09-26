// <copyright file="RoleCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
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

        var handler = new RoleCreatedEventHandler(_logger.Object, eventRepository.Object, publisher.Object);
        await handler.HandleAsync(message);

        publisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Exactly(3));
    }

    [Theory]
    [MemberData(nameof(Messages))]
    public async Task HandleAsync_WithNullOrEmptyMessage_Should_Return(string message)
    {
        var publisher = new Mock<IRoleEventPublisher>();
        var handler = new RoleCreatedEventHandler(_logger.Object, null!, publisher.Object);
        await handler.HandleAsync(message);

        publisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
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
