// <copyright file="BaseEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Broker.Events
{
    public class BaseEvent
    {
        public Guid EventId => Guid.NewGuid();

        public string? EventIdentifier { get; set; }

        public string? EventType => this.GetType().Name;

        public DateTime CreationDate => DateTime.UtcNow;

        public string? Sender { get; set; }
    }
}
