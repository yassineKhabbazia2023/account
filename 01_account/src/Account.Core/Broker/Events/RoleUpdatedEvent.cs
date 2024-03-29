// <copyright file="RoleUpdatedEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Pulse.Account.Core.Broker.Events.DataEvents;

namespace Pulse.Account.Core.Broker.Events
{
    public class RoleUpdatedEvent : BaseEvent
    {
        public RoleUpdatedDataEvent? DataEvent { get; set; }
    }
}
