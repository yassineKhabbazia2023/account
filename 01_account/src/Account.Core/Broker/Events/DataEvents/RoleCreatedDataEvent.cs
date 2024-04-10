// <copyright file="RoleCreatedDataEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Broker.Events.DataEvents;

public class RoleCreatedDataEvent
{
    public int AccountId { get; set; }

    public int ContactId { get; set; }

    public bool? IsSignatory { get; set; }

    public bool? IsFavorite { get; set; }

    public bool? IsDelegation { get; set; }
}
