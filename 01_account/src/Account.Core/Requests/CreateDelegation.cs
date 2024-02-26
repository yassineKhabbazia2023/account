// <copyright file="CreateDelegation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Requests;

public class CreateDelegation
{
    required public int AccountId { get; set; }

    required public int DelegatorId { get; set; }

    required public int DelegateeId { get; set; }

    required public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Note { get; set; }
}
