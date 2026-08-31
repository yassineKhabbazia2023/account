// <copyright file="INotificationManager.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.Abstractions;

namespace Pulse.Account.Core.Interfaces;

public interface INotificationManager
{
    Task PublishAsync<TEventData>(BaseEvent<TEventData> baseEvent, string? topicName = default!);
}
