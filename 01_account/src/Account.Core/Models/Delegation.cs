// <copyright file="Delegation.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.Contracts;

namespace Pulse.Account.Core.Models;

public class Delegation
{
    public int DelegationId { get; set; }

    public bool IsEnable { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime CreationDate { get; set; }

    public Account Account { get; set; }

    public Contact ContactDestination { get; set; }

    public Contact ContactSource { get; set; }
}
