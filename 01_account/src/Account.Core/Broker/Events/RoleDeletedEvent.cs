// <copyright file="RoleDeletedEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Broker.Events.DataEvents;

namespace Pulse.Account.Core.Broker.Events
{
    public class RoleDeletedEvent : BaseEvent
    {
        public RoleDeletedDataEvent? DataEvent { get; set; }
    }
}
