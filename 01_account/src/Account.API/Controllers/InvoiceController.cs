// <copyright file="InvoiceController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.API.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

    /// <summary>
    /// Get invoices for a specific account based on the provided criteria and pagination parameters.
    /// </summary>
    /// <param name="accountId">The ID of the account for which to retrieve invoices.</param>
    /// <param name="criteria">The criteria to filter the invoices.</param>
    /// <param name="pagination">The pagination parameters.</param>
    /// <returns>A paginated list of invoices for the specified account.</returns>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(Paging<Invoice>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Paging<Invoice>>> GetInvoicesAsync([FromRoute] int accountId, [FromQuery] SearchInvoicesCriteria criteria, [FromQuery] Pagination? pagination)
    {
        if (!InvoiceConstants.ValidSortByValues.Contains(criteria.SortBy, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest($"Invalid sortBy value. Allowed values are: {string.Join(", ", InvoiceConstants.ValidSortByValues)}");
        }

        if (!InvoiceConstants.ValidSortOrderValues.Contains(criteria.SortOrder, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest($"Invalid sortOrder value. Allowed values are: {string.Join(", ", InvoiceConstants.ValidSortOrderValues)}");
        }

        var result = await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        if (result.TotalItems == 0)
        {
            return NoContent();
        }

        return Ok(result);
    }
}
