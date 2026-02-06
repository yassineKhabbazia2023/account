// <copyright file="VentyaController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.API.Controllers;

[Route("api/ventya")]
[ApiController]
public class VentyaController : ControllerBase
{
    private readonly IVentyaService _ventyaService;

    public VentyaController(IVentyaService ventyaService)
    {
        _ventyaService = ventyaService;
    }

    /// <summary>
    /// Indique si l'entite est prete pour la dematerialisation.
    /// </summary>
    /// <param name="accountNumber">Le numero de compte.</param>
    /// <returns>Statut de disponibilite.</returns>
    [HttpGet("accounts/{accountNumber}/demat-ready")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DematReadyResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DematReadyResponse>> CheckAccountIsDematReadyAsync(string accountNumber)
    {
        var result = await _ventyaService.CheckAccountIsDematReadyAsync(accountNumber);

        if (result.Status == ResultStatus.NotFound)
        {
            return NotFound();
        }

        return Ok(result.Value!);
    }
}
