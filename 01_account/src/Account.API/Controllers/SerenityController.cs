// <copyright file="SerenityController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Account.API.Controllers;

[Route("api")]
[ApiController]
public class SerenityController : ControllerBase
{
    private readonly ISerenityService _serenityService;

    public SerenityController(ISerenityService serenityService)
    {
        _serenityService = serenityService;
    }

    /// <summary>
    /// Récupérer l'état d'éligibilité à la modal Sérénité pour le contact connecté.
    /// </summary>
    /// <remarks>
    /// Retourne si un choix a déjà été exprimé, et à défaut les entités du portefeuille du contact
    /// dont le code de routage vaut 0-B2B et dont l'adresse électronique est absente.
    /// Le critère de souscription est évalué par le micro-service Offer, pas ici.
    /// </remarks>
    /// <param name="currentUserId">ID du contact connecté (header CurrentUser).</param>
    /// <returns>L'état d'éligibilité Sérénité.</returns>
    [HttpGet("serenity-eligibility")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SerenityEligibility))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<SerenityEligibility>> GetSerenityEligibilityAsync([FromHeader(Name = "CurrentUser")] int currentUserId)
    {
        var result = await _serenityService.GetSerenityEligibilityAsync(currentUserId);

        return Ok(result);
    }

    /// <summary>
    /// Enregistrer le choix du contact connecté sur la modal Sérénité.
    /// </summary>
    /// <remarks>Le choix est immuable : un second appel retourne 409 Conflict.</remarks>
    /// <param name="currentUserId">ID du contact connecté (header CurrentUser).</param>
    /// <param name="request">Le choix exprimé.</param>
    /// <returns>204 No Content si le choix a été enregistré.</returns>
    [HttpPost("serenity-choice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateSerenityChoiceAsync(
        [FromHeader(Name = "CurrentUser")][Range(1, int.MaxValue)] int currentUserId,
        [FromBody] SerenityChoiceRequest request)
    {
        await _serenityService.CreateSerenityChoiceAsync(currentUserId, request.IsAccepted);

        return NoContent();
    }
}
