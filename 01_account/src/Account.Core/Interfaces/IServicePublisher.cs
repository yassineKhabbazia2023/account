// <copyright file="IServicePublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Broker.Events;

namespace Pulse.Account.Core.Interfaces
{
    public interface IServicePublisher
    {
        Task PublishAsync(BaseEvent @event);
    }
}
