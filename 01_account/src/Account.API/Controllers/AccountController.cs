// <copyright file="AccountController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;
using Pulse.ExceptionMiddleware.Model;

using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.API.Controllers;

[Route("api/accounts")]
[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
    /// <summary>
    /// Créer une entité morale.
    /// </summary>
    /// <param name="currentUserId">ID de l'utilisateur actuel.</param>
    /// <param name="request">Informations de création.</param>
    /// <returns>Entité morale créée.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateAccountResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<CreateAccountResponse>> CreateAccountAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdAccount = await accountService.CreateAccountAsync(currentUserId, request);

        var response = new CreateAccountResponse
        {
            Message = "Compte cree avec succes",
            AccountId = createdAccount.AccountId
        };

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Recherche des entités morales.
    /// </summary>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <param name="currentUserEmail">Email de l'utilisateur connecté (header ContactEmail).</param>
    /// <returns>Liste d'entités morales.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<AccountModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Paging<AccountModel>>> GetAccountsAsync([FromQuery] SearchAccountCriteria criteria,
        [FromQuery] Pagination? pagination,
        [FromHeader(Name = "ContactEmail")] string? currentUserEmail = null)
    {
        var result = await accountService.GetAccountsAsync(criteria, pagination, currentUserEmail);

        return Ok(result);
    }

    /// <summary>
    /// Lister toutes les entités morales de la BD.
    /// </summary>
    /// <param name="accountNumber">AccountNumber de l'entité morale.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <param name="criteria">Paramètres de sort.</param>
    /// <returns>Liste d'entités morales.</returns>
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<AccountModel>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<AccountModel>>> GetAllAccountsAsync(
        string? accountNumber,
        [FromQuery] Pagination? pagination,
        [FromQuery] SearchAccountCriteria? criteria)
    {
        var result = await accountService.GetAllAccountsAsync(accountNumber, pagination, criteria);

        return Ok(result);
    }

    /// <summary>
    /// Rechercher des entités morales par raison sociale ou code client.
    /// </summary>
    /// <param name="keyword">Texte à rechercher dans LegalName ou AccountNumber.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste paginée d'entités morales correspondant à la recherche (accountId, legalName, accountNumber, SIRET, email dirigeant).</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<AccountSearchResult>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<AccountSearchResult>>> SearchAccountsAsync(
        string? keyword,
        [FromQuery] Pagination? pagination)
    {
        var result = await accountService.SearchAccountsAsync(keyword, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Récupérer les informations détaillées d'une entité morale, enrichies de la date de fermeture du modal
    /// de collecte du mail demat par le contact courant.
    /// </summary>
    /// <param name="accountId">ID de l'entité morale.</param>
    /// <param name="currentUser">L'identifiant du contact courant (depuis le header), si disponible. Absent pour un appel sans contexte utilisateur (job, datafactory...) : <see cref="AccountDetail.ModalClosedAt"/> reste alors non renseigné.</param>
    /// <returns>Informations détaillées de l'entité morale, avec <see cref="AccountDetail.ModalClosedAt"/>.</returns>
    [HttpGet("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<AccountDetail>> GetAccountDetailAsync(
        int accountId,
        [FromHeader(Name = "CurrentUser")] int? currentUser)
    {
        var result = await accountService.GetAccountDetailAsync(accountId, currentUser);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Ferme le modal de collecte du mail demat pour le contact courant.
    /// </summary>
    /// <param name="accountId">L'id de compte.</param>
    /// <param name="currentUser">L'identifiant du contact à l'origine de la fermeture (depuis le header).</param>
    /// <returns>204 si la fermeture est enregistrée, 404 si le compte n'existe pas.</returns>
    [HttpPost("{accountId:int:min(1)}/demat-ready/closure")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseDematModalAsync(
        [FromRoute] int accountId,
        [FromHeader(Name = "CurrentUser")][Required] int currentUser)
    {
        var result = await accountService.CloseDematModalAsync(accountId, currentUser);

        if (result.Status == ResultStatus.NotFound)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Mettre à jour partiellement les informations d'une entité morale.
    /// </summary>
    /// <param name="accountId">ID de l'entité morale.</param>
    /// <param name="accountPatch">Informations à mettre à jour.</param>
    /// <param name="currentUserEmail">Email de l'utilisateur connecté (header ContactEmail).</param>
    /// <returns>Les informations détaillées de l'entité morale mises à jour.</returns>
    [HttpPatch("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateAccountAsync(int accountId, [FromBody] JsonPatchDocument<AccountDetail> accountPatch, [FromHeader(Name = "ContactEmail")] string? currentUserEmail = null)
    {
        if (accountPatch == null)
        {
            throw new BadRequestException(Errors.BadRequestAccountPatchCode, Errors.BadRequestAccountPatchMessage);
        }

        // Validation automatique grâce à ApiController
        if (!ModelState.IsValid)
        {
            // Erreurs automatiquement collectées
            return BadRequest(ModelState);
        }

        var accountToUpdate = await accountService.GetAccountAsync(accountId);
        if (accountToUpdate == null)
        {
            return NotFound();
        }

        accountPatch.ApplyTo(accountToUpdate, ModelState);
        var results = new List<ValidationResult>();
        bool isValid = AnnotationValidator.TryValidateObjectRecursive(accountToUpdate, results);

        if (!isValid)
        {
            foreach (var validationResult in results)
            {
                ModelState.AddModelError(
                    validationResult.MemberNames.FirstOrDefault() ?? "AccountDetail",
                    validationResult.ErrorMessage ?? "Validation failed");
            }

            return BadRequest(ModelState);
        }

        await accountService.UpdateAccountAsync(accountId, accountToUpdate!, currentUserEmail);

        return Ok();
    }

    /// <summary>
    /// Récupère un résumé des informations d'une entité morale à partir de son identifiant.
    /// </summary>
    /// <param name="currentUserId">Identifiant de l'utilisateur connecté.</param>
    /// <param name="contactType">Type de contact de l'utilisateur connecté.</param>
    /// <param name="accountId">Identifiant unique de l'entité morale.</param>
    /// <returns>Un résumé des informations de l'entité morale.</returns>
    [HttpGet("{accountId}/summary")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<AccountModel>> GetAccountSummaryAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [FromHeader(Name = "ContactType")] string contactType, int accountId)
    {
        var result = await accountService.GetAccountSummaryAsync(currentUserId, accountId, contactType);
        return Ok(result);
    }

    /// <summary>
    /// Récupérer les contacts responsable compte et maître dossier d'une entité morale.
    /// </summary>
    /// <param name="accountId">ID de l'entité morale.</param>
    /// <returns>La liste des contacts responsable compte et maître dossier.</returns>
    [HttpGet("{accountId}/contacts/widget")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Contact>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<Contact>>> GetAccountContactWidgetContactsAsync(int accountId)
    {
        var result = await accountService.GetAccountContactWidgetContactsAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    /// Récupérer la liste des contacts d'une entité morale.
    /// </summary>
    /// <param name="accountId">ID de l'entité morale.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="pagination">Paramètre de pagination.</param>
    /// <param name="currentUserEmail">Email de l'utilisateur connecté (header ContactEmail).</param>
    /// <returns>La liste des contacts.</returns>
    [HttpGet("{accountId}/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Contact>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<Contact>>> GetContactsAccountAsync(int accountId, [FromQuery] SearchContactsAccountCriteria criteria, [FromQuery] Pagination? pagination, [FromHeader(Name = "ContactEmail")] string? currentUserEmail = null)
    {
        var result = await accountService.GetContactsAccountAsync(accountId, criteria, pagination, currentUserEmail);
        return Ok(result);
    }

    /// <summary>
    /// Vérifier si un contact a uniquement des rôles sur des entités prospectes.
    /// </summary>
    /// <param name="contactId">Identifiant du contact.</param>
    /// <returns><c>true</c> si le contact existe, possède au moins un rôle et tous ses rôles sont sur des entités prospectes; sinon <c>false</c>.</returns>
    [HttpGet("contacts/{contactId}/is-prospect-only")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<bool>> IsContactProspectOnlyAsync(int contactId)
    {
        var result = await accountService.IsContactProspectOnlyAsync(contactId);
        return Ok(result);
    }

    /// <summary>
    /// Récupérer la liste des contacts rattachés aux entités d'un contact admin.
    /// </summary>
    /// <param name="contactId">Identifiant du contact connecté.</param>
    /// <param name="request">Paramètre de la requête.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>La liste des contacts rattachés aux entités d'un contact admin.</returns>
    [HttpGet("contacts")]
    [ProducesResponseType(typeof(IEnumerable<Contact>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<Contact>>> GetAssociatedContactsAsync(int contactId,
        [FromQuery] GetAssociatedContactsRequest request,
        [FromQuery] Pagination? pagination)
    {
        var result = await accountService.GetAssociatedContactsAsync(contactId, request, pagination);
        return Ok(result);
    }
}
