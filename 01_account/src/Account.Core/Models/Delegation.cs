// <copyright file="Delegation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Models;

public class Delegation
{
    [JsonIgnore]
    public int DelegationId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreationDate { get; set; }

    public IEnumerable<Account> Accounts { get; set; } = Enumerable.Empty<Account>();

    public Contact? Delegatee { get; set; }

    public Contact? Delegator { get; set; }

    public bool IsFullDelegation { get; set; }

    public bool IsAutomaticDelegation { get; set; }

    public bool IncludePennylaneAccess { get; set; }
}
