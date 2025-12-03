// <copyright file="DelegationController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Model;

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
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
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
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetDelegationsAsync([FromQuery] int delegatorId, [FromQuery] int delegateeId)
    {
        var delegations = await _delegationService.GetDelegationsAsync(delegatorId, delegateeId);
        return Ok(delegations!);
    }

    /// <summary>
    /// Ajouter une délégation sur une entité morale.
    /// </summary>
    /// <param name="contactId">Identification de l'utilisateur connecté.</param>
    /// <param name="delegation">Le détail relatif à la délégation.</param>
    /// <returns>Un entitier positif si la délégation a été bien ajouter, sinon une valeur 0. </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> CreateDelegationAsync([Required] int contactId, [FromBody] CreateDelegationRequest delegation)
    {
        await _delegationService.CreateDelegationAsync(contactId, delegation);

        return Ok();
    }

    /// <summary>
    /// Supprime une délégation sur une entité morale.
    /// </summary>
    /// <param name="currentUserId">L'identifiant de l'utilisateur connecté.</param>
    /// <param name="delegationId">L'identifiant de la délégation.</param>
    /// <param name="delegatorId">L'identifiant du contact délégateur.</param>
    /// <param name="delegateeId">L'identifiant du contact délégataire.</param>
    /// <returns>Ok si la suppression s'est bien déroulée.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> DeleteDelegationAsync([FromHeader(Name = "CurrentUser")][Required] int currentUserId, int delegationId, int delegatorId, int delegateeId)
    {
        await _delegationService.DeleteDelegationAsync(currentUserId, delegationId, delegatorId, delegateeId);

        return Ok();
    }

    /// <summary>
    /// Récupérer l'historique des délégations d'une entité morale.
    /// </summary>
    /// <param name="accountId">L'identifiant de l'identité morale.</param>
    /// <param name="search">Critère de recherche (nom/prénom du délégateur ou du délégataire).</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste de délégations.</returns>
    [HttpGet("{accountId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<Delegation>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<Delegation>>> GetAccountDelegationsHistoryAsync(int accountId,
        string? search,
        [FromQuery] Pagination? pagination)
    {
        var delegations = await _delegationService.GetAccountDelegationsHistoryAsync(accountId, search, pagination);
        return Ok(delegations!);
    }

    /// <summary>
    /// Récupérer l'historique des délégations d'un contact.
    /// </summary>
    /// <param name="contactId">L'identifiant du contact.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <param name="sortAscending">Sens de sort colonne nom.</param>
    /// <returns>Liste de délégations.</returns>
    [HttpGet("{contactId}/historyDelegation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<Delegation>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<Delegation>>> GetContactDelegationsHistoryAsync(int contactId,
        [FromQuery] Pagination? pagination,
        bool sortAscending = true)
    {
        var delegations = await _delegationService.GetContactDelegationsHistoryAsync(contactId, pagination, sortAscending);
        return Ok(delegations!);
    }
}
