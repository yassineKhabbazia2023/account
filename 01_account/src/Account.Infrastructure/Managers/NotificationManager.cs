// <copyright file="NotificationManager.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Back.Events.Abstractions;

namespace Pulse.Account.Infrastructure.Managers;

public class NotificationManager(IEventPublisher eventPublisher) : INotificationManager
{
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public async Task PublishAsync<TEventData>(BaseEvent<TEventData> baseEvent, string? topicName = default)
    {
        ArgumentNullException.ThrowIfNull(baseEvent);
        await _eventPublisher.PublishAsync(baseEvent, null!, topicName);
    }
}
