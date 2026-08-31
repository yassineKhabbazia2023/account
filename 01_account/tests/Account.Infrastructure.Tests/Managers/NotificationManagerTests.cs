// <copyright file="NotificationManagerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Notifications.Commons.WebApi.QueryParams;
using Pulse.Account.Infrastructure.Managers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;

namespace Pulse.Account.Infrastructure.Tests.Managers;

public class NotificationManagerTests
{
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly NotificationManager _manager;

    public NotificationManagerTests()
    {
        _mockEventPublisher = new Mock<IEventPublisher>();
        _manager = new NotificationManager(_mockEventPublisher.Object);
    }

    [Fact]
    public async Task PublishAsync_ShouldForwardEventToEventPublisherWithTopic()
    {
        var baseEvent = CreateEvent();
        const string topicName = "email-topic";

        await _manager.PublishAsync(baseEvent, topicName);

        _mockEventPublisher.Verify(
            p => p.PublishAsync(baseEvent, null!, topicName),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenTopicNotProvided_ShouldForwardNullTopic()
    {
        var baseEvent = CreateEvent();

        await _manager.PublishAsync(baseEvent);

        _mockEventPublisher.Verify(
            p => p.PublishAsync(baseEvent, null!, null),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenEventIsNull_ShouldThrowArgumentNullException()
    {
        BaseEvent<EmailRequest> baseEvent = null!;

        Func<Task> act = async () => await _manager.PublishAsync(baseEvent);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    private static EmailOnlySenderEvent CreateEvent()
    {
        var emailRequest = new EmailRequest
        {
            From = "noreply@pulse.test",
            To = new List<string> { "recipient@pulse.test" },
            TemplateName = "template",
            Variables = new Dictionary<string, object>()
        };

        return new EmailOnlySenderEvent(emailRequest);
    }
}
