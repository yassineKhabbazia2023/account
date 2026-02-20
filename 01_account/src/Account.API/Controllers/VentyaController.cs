// <copyright file="VentyaController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.API.Controllers;

/// <summary>
/// Les différents endpoints pour l'intégration Ventya.
/// </summary>
[ApiController]
[Route("api/ventya")]
public class VentyaController : ControllerBase
{
    private readonly IVentyaService _ventyaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VentyaController"/> class.
    /// </summary>
    /// <param name="ventyaService">Le service Ventya.</param>
    public VentyaController(IVentyaService ventyaService)
    {
        _ventyaService = ventyaService;
    }

    /// <summary>
    /// Vérifie si l'utilisateur courant a accès au coffre Ventya pour un compte donné.
    /// </summary>
    /// <param name="accountId">L'identifiant du compte.</param>
    /// <param name="currentUser">L'identifiant du contact courant (depuis le header).</param>
    /// <returns>Un objet indiquant si l'utilisateur courant a accès au coffre Ventya.</returns>
    [HttpGet("accounts/{accountId:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<VentyaAccessResponse>> CheckCurrentUserVentyaAccessAsync(
        [FromRoute] int accountId,
        [FromHeader(Name = "CurrentUser")][Required] int currentUser)
    {
        var hasAccess = await _ventyaService.CheckVentyaAccessAsync(accountId, currentUser);
        string? contactWithAccess = null;

        if (!hasAccess)
        {
            contactWithAccess = await _ventyaService.GetVentyaAccessContactEmailAsync(accountId);
            return Ok(new VentyaAccessResponse { HasAccess = hasAccess, ContactWithAccess = contactWithAccess });
        }

        return Ok(new VentyaAccessResponse { HasAccess = hasAccess });
    }

    /// <summary>
    /// Indique si l'entite est prete pour la dematerialisation.
    /// </summary>
    /// <param name="accountId">L'id de compte.</param>
    /// <returns>Statut de disponibilite.</returns>
    [HttpGet("accounts/{accountId:int:min(1)}/demat-ready")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DematReadyResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DematReadyResponse>> CheckAccountIsDematReadyAsync(int accountId)
    {
        var result = await _ventyaService.CheckAccountIsDematReadyAsync(accountId);

        if (result.Status == ResultStatus.NotFound)
        {
            return NotFound();
        }

        return Ok(result.Value!);
    }
}

/// <summary>
/// Réponse pour l'accès au coffre Ventya.
/// </summary>
public class VentyaAccessResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the contact has access to the Ventya vault.
    /// </summary>
    public bool HasAccess { get; set; }

    /// <summary>
    /// Gets or sets the contact email that has access to Ventya when the current contact does not.
    /// </summary>
    public string? ContactWithAccess { get; set; }
}
