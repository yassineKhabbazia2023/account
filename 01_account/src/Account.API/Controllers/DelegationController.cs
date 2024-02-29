// <copyright file="DelegationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.API.Controllers;

/// <summary>
/// Les différents endpoints pour la gestion délégations.
/// </summary>
[ApiController]
[Route("api/delegations")]
public class DelegationController : ControllerBase
{
    private readonly IDelegationService _delegationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegationController"/> class.
    /// </summary>
    /// <param name="delegationService">Une instance of delegation service.</param>
    public DelegationController(IDelegationService delegationService)
    {
        _delegationService = delegationService;
    }

    /// <summary>
    /// Récupérer les délégations d'un contact donné.
    /// </summary>
    /// <param name="delegateeId">L'identifiant du contact.</param>
    /// <returns>Liste de délégations.</returns>
    [HttpGet("{delegateeId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Delegation>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetContactDelegationsAsync(int delegateeId)
    {
        var delegations = await _delegationService.GetContactDelegationsAsync(delegateeId);
        return Ok(delegations!);
    }

    /// <summary>
    /// Récupérer les délégations accordées par un contact à un autre contact.
    /// </summary>
    /// <param name="delegatorId">L'identifiant du contact délégateur.</param>
    /// <param name="delegateeId">L'identifiant du contact délégataire.</param>
    /// <returns>Liste de délégations.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Delegation>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetDelegationsAsync([FromQuery] int delegatorId, [FromQuery] int delegateeId)
    {
        var delegations = await _delegationService.GetDelegationsAsync(delegatorId, delegateeId);
        return Ok(delegations!);
    }

    /// <summary>
    /// Ajouter une délégation sur une entité morale.
    /// </summary>
    /// <param name="delegation">Le détail relatif à la délégation.</param>
    /// <returns>Un entitier positif si la délégation a été bien ajouter, sinon une valeur 0. </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateDelegationAsync([FromBody] CreateDelegationRequest delegation)
    {
        int delegationId = await _delegationService.CreateDelegationAsync(delegation);

        return Ok(delegationId);
    }

    /// <summary>
    /// Supprime une délégation sur une entité morale.
    /// </summary>
    /// <param name="delegationId">L'identifiant de la délégation.</param>
    /// <returns>Ok si la suppression s'est bien déroulée.</returns>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDelegationAsync(int delegationId)
    {
        await _delegationService.DeleteDelegationAsync(delegationId);

        return Ok();
    }
}
