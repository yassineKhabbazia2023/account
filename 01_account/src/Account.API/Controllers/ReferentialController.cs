// <copyright file="ReferentialController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Account.API.Controllers;

[Route("api/referentials")]
[ApiController]
public class ReferentialController : ControllerBase
{
    private readonly IReferentialService _referentialService;

    public ReferentialController(IReferentialService referentialService)
    {
        _referentialService = referentialService;
    }

    /// <summary>
    /// Récupère la liste des hubs.
    /// </summary>
    /// <param name="sort">Tri optionnel sur le HubName (ASC/DESC).</param>
    /// <returns>Liste de hubs.</returns>
    [HttpGet("hubs")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Hub?>))]
    public async Task<ActionResult<IReadOnlyCollection<Hub?>>> GetHubsAsync([FromQuery] string? sort = null)
    {
        var result = await _referentialService.GetHubsAsync(sort);

        return Ok(result);
    }

    /// <summary>
    /// Récupère la liste des NAF.
    /// </summary>
    /// <param name="search">Critère de recherche (code NAF).</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste des NAF.</returns>
    [HttpGet("nafs")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Naf?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IReadOnlyCollection<Naf?>>> GetNafsAsync(string? search, [FromQuery] Pagination? pagination)
    {
        var result = await _referentialService.GetNafsAsync(search, pagination);

        return Ok(result);
    }

    /// <summary>
    /// Récupère les données de référence concernant les entités morales.
    /// </summary>
    /// <returns>les information de reference des entités morales.</returns>
    [HttpGet("AccountReferentialInformation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountReferentialInformation))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public ActionResult<AccountReferentialInformation> GetAccountReferentialInformation()
    {
        var result = _referentialService.GetAccountReferentialInformation();

        return Ok(result);
    }

    /// <summary>
    /// Récupère les données de référence concernant les bureaux.
    /// </summary>
    /// <returns>les information de reference des entités morales.</returns>
    [HttpGet("offices")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Office>))]
    public async Task<ActionResult<IEnumerable<Office?>>> GetOfficesAsync()
    {
        var result = await _referentialService.GetOfficesAsync();

        return Ok(result);
    }
}
