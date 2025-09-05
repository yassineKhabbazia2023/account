// <copyright file="OfferEligibility.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class OfferEligibility
{
    public int AccountId { get; set; }

    public string OfferName { get; set; } = string.Empty;

    public bool IsEligible { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? ApprovedBy { get; set; }
}
