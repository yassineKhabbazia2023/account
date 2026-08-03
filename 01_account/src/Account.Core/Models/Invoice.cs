// <copyright file="Invoice.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class Invoice
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public required string Type { get; set; }

    public required string Category { get; set; }

    public required DateTime DepositDate { get; set; }

    public required int InvoiceYear { get; set; }
}
