// <copyright file="OfferEligibilityController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Account.API.Controllers;

[Route("api")]
[ApiController]
public class OfferEligibilityController : ControllerBase
{
    private readonly IOfferEligibilityService _offerEligibilityService;

    public OfferEligibilityController(IOfferEligibilityService offerEligibilityService)
    {
        _offerEligibilityService = offerEligibilityService;
    }

    /// <summary>
    /// Mettre à jour partiellement les informations d'un offerEligibility.
    /// </summary>
    /// <param name="currentUserId">ID de l'utilisateur connecté.</param>
    /// <param name="accountId">ID de l'entité morale.</param>
    /// <returns>Les informations détaillées de l'offerEligibility mises à jour.</returns>
    [HttpPatch("activate-offer/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OfferEligibility))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateOfferEligibilityAsync([FromHeader(Name = "CurrentUser")] int currentUserId, int accountId)
    {
        var entityUpdated = await _offerEligibilityService.UpdateOfferEligibilityAsync(currentUserId, accountId);

        return Ok(entityUpdated);
    }
}
