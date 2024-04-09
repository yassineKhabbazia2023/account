// <copyright file="RoleUpdatedDataEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Broker.Events.DataEvents
{
    public class RoleUpdatedDataEvent
    {

        public int AccountId { get; set; }

        public int ContactId { get; set; }

        public bool? IsSignatory { get; set; }
    }
}
