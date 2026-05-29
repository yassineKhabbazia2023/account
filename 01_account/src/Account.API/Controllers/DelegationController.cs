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
/// Les différents endpoints pour la gestion des délégations et leurs demandes.
/// </summary>
[ApiController]
[Route("api/delegations")]
public class DelegationController : ControllerBase
{
    private readonly IDelegationService _delegationService;
    private readonly IDelegationRequestService _delegationRequestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegationController"/> class.
    /// </summary>
    /// <param name="delegationService">Une instance of delegation service.</param>
    /// <param name="delegationRequestService">Une instance du service de demandes de délégation.</param>
    public DelegationController(IDelegationService delegationService, IDelegationRequestService delegationRequestService)
    {
        _delegationService = delegationService;
        _delegationRequestService = delegationRequestService;
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
    /// Récupérer les délégations accordées par le délégateur connecté.
    /// </summary>
    /// <param name="contactId">L'identifiant du contact délégateur (injecté par la gateway).</param>
    /// <param name="filter">Filtres optionnels sur les délégations.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste paginée de délégations.</returns>
    [HttpGet("delegator")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<Delegation>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<Delegation>>> GetDelegatorDelegationsAsync(
        [FromQuery] int contactId,
        [FromQuery] DelegationFilter filter,
        [FromQuery] Pagination? pagination)
    {
        var delegations = await _delegationService.GetDelegatorDelegationsAsync(contactId, filter, pagination);
        return Ok(delegations);
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
    /// <param name="delegationId">L'identifiant de la délégation.</param>
    /// <returns>Ok si la suppression s'est bien déroulée.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> DeleteDelegationAsync(int delegationId)
    {
        await _delegationService.DeleteDelegationAsync(delegationId);

        return Ok();
    }

    // ========== Endpoints pour les demandes de délégation (workflow d'approbation) ==========

    /// <summary>
    /// Créer des demandes de délégation pour un dossier.
    /// </summary>
    /// <param name="currentUserId">L'identifiant du contact demandeur (utilisateur connecté).</param>
    /// <param name="request">Les informations de la demande (AccountId et RecipientIds).</param>
    /// <returns>201 Created si les demandes ont été créées avec succès.</returns>
    [HttpPost("requests")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> CreateDelegationRequestsAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [FromBody] CreateDelegationRequestsRequest request)
    {
        await _delegationRequestService.CreateDelegationRequestsAsync(currentUserId, request);

        return Created();
    }

    /// <summary>
    /// Récupérer les demandes de délégation envoyées par l'utilisateur connecté.
    /// </summary>
    /// <param name="currentUserId">L'identifiant du contact (utilisateur connecté).</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste paginée des demandes de délégation envoyées.</returns>
    [HttpGet("requests/sent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<DelegationRequest>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<DelegationRequest>>> GetSentRequestsAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [FromQuery] Pagination? pagination)
    {
        var result = await _delegationRequestService.GetSentRequestsAsync(currentUserId, pagination);

        return Ok(result);
    }

    /// <summary>
    /// Récupérer les demandes de délégation reçues par l'utilisateur connecté (statut: pending).
    /// </summary>
    /// <param name="currentUserId">L'identifiant du contact (utilisateur connecté).</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste paginée des demandes de délégation reçues en attente.</returns>
    [HttpGet("requests/received")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<DelegationRequest>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<DelegationRequest>>> GetReceivedRequestsAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [FromQuery] Pagination? pagination)
    {
        var result = await _delegationRequestService.GetReceivedRequestsAsync(currentUserId, pagination);

        return Ok(result);
    }

    /// <summary>
    /// Vérifier si l'utilisateur peut demander une délégation sur un dossier donné.
    /// </summary>
    /// <param name="currentUserId">L'identifiant du contact (utilisateur connecté).</param>
    /// <param name="accountId">L'identifiant du dossier.</param>
    /// <returns>Réponse d'éligibilité (isEligible, reason).</returns>
    [HttpGet("requests/eligibility")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DelegationEligibilityResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<DelegationEligibilityResponse>> CheckEligibilityAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [FromQuery] int accountId)
    {
        var result = await _delegationRequestService.CheckEligibilityAsync(currentUserId, accountId);

        return Ok(result);
    }
}
