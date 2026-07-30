// <copyright file="CreateInvoiceRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests;

/// <summary>
/// Request object used to create an invoice.
/// </summary>
public class CreateInvoiceRequest
{
    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public required string InvoiceNumber { get; set; }

    /// <summary>
    /// Gets or sets the invoice name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    public DateTime? InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the invoice deposit date.
    /// </summary>
    public DateTime? DepositDate { get; set; }

    /// <summary>
    /// Gets or sets the related account identifier.
    /// </summary>
    public required int AccountId { get; set; }
}
