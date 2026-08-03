// <copyright file="InvoiceController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<Paging<Invoice>>> GetInvoicesAsync([FromRoute] int accountId, [FromQuery] SearchInvoicesCriteria criteria, [FromQuery] Pagination? pagination)
    {
        var result = await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        if (result.TotalItems == 0)
        {
            return NoContent();
        }

        return Ok(result);
    }
}
