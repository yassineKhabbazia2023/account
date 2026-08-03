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
    /// Gets or sets the document path, in order to download the invoice.
    /// </summary>
    public required string DocumentPath { get; set; }

    /// <summary>
    /// Gets or sets the invoice type.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the invoice category.
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    public required DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the invoice deposit date (optional - defaults to GETDATE() in database).
    /// </summary>
    public required DateTime DepositDate { get; set; }

    /// <summary>
    /// Gets or sets the related account identifier.
    /// </summary>
    public required int AccountId { get; set; }
}
