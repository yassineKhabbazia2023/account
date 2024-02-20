// <copyright file="Delegation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Pulse.Account.Core.Constants;

namespace Pulse.Account.Core.Models;

public class Delegation
{
    [JsonIgnore]
    public int DelegationId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DelegationStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreationDate { get; set; }

    public Account? Account { get; set; }

    public Contact? Delegatee { get; set; }

    public Contact? Delegator { get; set; }
}
