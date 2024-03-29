// <copyright file="CreatedRoleEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Broker.Events
{
    public class CreatedRoleEvent : BaseEvent
    {
        [JsonPropertyName("dataEvent")]
        public CreatedRoleDataEvent? DataEvent { get; set; }
    }
}
