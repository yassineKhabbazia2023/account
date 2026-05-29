// <copyright file="DelegationEligibilityResponse.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class DelegationEligibilityResponse
{
    public bool IsEligible { get; set; }

    public string? Reason { get; set; }
}