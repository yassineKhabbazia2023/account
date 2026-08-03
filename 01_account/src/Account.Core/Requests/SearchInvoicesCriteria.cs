// <copyright file="SearchInvoicesCriteria.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Constants;

namespace Pulse.Account.Core.Requests;

public class SearchInvoicesCriteria
{
    public string? Search { get; set; }

    public string SortBy { get; set; } = InvoiceConstants.DefaultSorting;

    public string SortOrder { get; set; } = InvoiceConstants.DefaultSortingOrder;
}
