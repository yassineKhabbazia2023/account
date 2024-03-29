// <copyright file="RoleCreatedEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Pulse.Account.Core.Broker.Events.DataEvents;

namespace Pulse.Account.Core.Broker.Events
{
    public class RoleCreatedEvent : BaseEvent
    {
        public RoleCreatedDataEvent? DataEvent { get; set; }
    }
}
