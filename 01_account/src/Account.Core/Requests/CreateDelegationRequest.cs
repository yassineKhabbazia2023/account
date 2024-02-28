// <copyright file="CreateDelegationRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests;

public class CreateDelegationRequest
{
    required public int AccountId { get; set; }

    required public int DelegatorId { get; set; }

    required public int DelegateeId { get; set; }

    required public DateTime? StartDate { get; set; }

    required public string Status { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Note { get; set; }
}
