// <copyright file="RolesController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Account.API.Controllers;

/// <summary>
/// Les différents endpoints pour la gestion rôles.
/// </summary>
[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesController"/> class.
    /// </summary>
    /// <param name="rolesService">Une instance of role service.</param>
    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    /// <summary>
    /// Lister les entités morales auxquelles un contact est lié.
    /// </summary>
    /// <param name="contactId">Identification de l'utilisateur connecté.</param>
    /// <param name="pagination">Paramètres de pagination.</param>
    /// <returns>Liste d'entités morales.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Paging<Core.Models.Account>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Paging<Core.Models.Account>>> GetContactRolesAsync(int contactId, [FromQuery] Pagination? pagination)
    {
        var result = await _rolesService.GetContactRolesAsync(contactId, pagination);

        return Ok(result);
    }

    /// <summary>
    /// Récupérer la liste des signataires d'une entité morale.
    /// </summary>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>La liste des signataires.</returns>
    [HttpGet("signatory/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<Contact>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<IEnumerable<Contact>>> GetSignatoryAsync(int accountId)
    {
        var result = await _rolesService.GetSignatoryAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    /// Vérifier si un contact a un rôle sur l’account/les accounts auxquels l’utilisateur connecté a accès.
    /// </summary>
    /// <param name="contactId">Identifiant de l'utilisateur.</param>
    /// <param name="accountId">Identifiant de l'entitié morale.</param>
    /// <param name="email">L'email de l'utilisateur.</param>
    /// <returns>http 200.</returns>
    [HttpGet("check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> CheckRoleExists(int? contactId, int? accountId, string? email)
    {
        if (!Request.Headers.TryGetValue("CurrentUser", out StringValues contactIdValue))
        {
            throw new BadRequestException(Errors.CurrentUserWasNotFoundInHeadersCode, Errors.CurrentUserWasNotFoundInHeadersMessage);
        }

        if (!int.TryParse(contactIdValue, out int currentUserId))
        {
            throw new BadRequestException(Errors.InvalidCurrentUserFormatCode, Errors.InvalidCurrentUserFormatMessage);
        }

        if (!contactId.HasValue && string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(Errors.ContactIdAndEmailNullCode, Errors.ContactIdAndEmailNullMessage);
        }

        var contactHasRoleOnAccount = await _rolesService.CheckRoleExistsAsync(currentUserId, contactId, accountId, email);

        return Ok(contactHasRoleOnAccount);
    }

    /// <summary>
    /// Vérifier si un contact a un rôle sur un account donné.
    /// </summary>
    /// <param name="contactId">Identifiant de l'utilisateur.</param>
    /// <param name="accountId">Identifiant de l'entitié morale.</param>
    /// <param name="accountNumber">Identifiant fonctionnel de l'entité morale.</param>
    /// <returns>http 200.</returns>
    [HttpGet("check-contact-role-on-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> IsContactHasRoleOnAccount([Required] int contactId, int? accountId, string? accountNumber)
    {
        if(!accountId.HasValue && string.IsNullOrEmpty(accountNumber))
        {
            throw new BadRequestException(Errors.BadRequestAccountIdAndAccountNumberNullCode, Errors.BadRequestAccountIdAndAccountNumberNullMessage);
        }

        var contactHasRoleOnAccount = await _rolesService.IsContactHasRoleOnAccount(contactId, accountId, accountNumber);

        return Ok(contactHasRoleOnAccount);
    }

    /// <summary>
    /// Créer un role pour un contact dans une entité morale.
    /// </summary>
    /// <param name="currentUserId">L'identifiant de l'utilisateur courant.</param>
    /// <param name="accountId">L'identifiant de l'entité.</param>
    /// <param name="role">Objet role qui va lier un contact à une entité morale.</param>
    /// <returns>http 201.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> CreateRoleAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [Required] int accountId, [Required] CreateRoleRequest role)
    {
        if (role != null)
        {
            role.AccountId = accountId;
        }

        await _rolesService.CreateRoleAsync(role!, currentUserId);
        return Created();
    }

    /// <summary>
    /// Créer plusieurs roles pour une entité morale à partir d'une liste de contacts.
    /// </summary>
    /// <param name="currentUserId">L'identifiant de l'utilisateur courant.</param>
    /// <param name="accountId">L'identifiant de l'entité morale.</param>
    /// <param name="request">Liste des contacts à rattacher avec leurs options (signataire, favori, délégation, RoleCode optionnel "AM"/"CLP" pour assigner le label correspondant, ...).</param>
    /// <returns>Résultat par contact : succès et échecs.</returns>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(CreateRolesBulkResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<CreateRolesBulkResult>> CreateRolesBulkAsync(
        [FromHeader(Name = "CurrentUser")] int currentUserId,
        [Required] int accountId,
        [Required][FromBody] CreateRolesBulkRequest request)
    {
        var result = await _rolesService.CreateRolesBulkAsync(accountId, request, currentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Modifier un role pour un contact.
    /// </summary>
    /// <param name="currentUserId">Identifiant de l'utilisateur courant.</param>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <param name="contactId">Identifiant de l'utilisateur.</param>
    /// <param name="isSignatory">True si l'utilisateur est signataire, false sinon.</param>
    /// <returns>http 200.</returns>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> UpdateRoleSignatoryAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [Required] int accountId, [Required] int contactId, [Required] bool isSignatory)
    {
        await _rolesService.UpdateRoleSignatoryAsync(currentUserId, accountId, contactId, isSignatory);
        return Ok();
    }

    [HttpPatch("customer-relation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> UpdateRoleCustomerRelationAsync([Required] int accountId, [Required] int contactId, [Required] bool isCustomerRelation)
    {
        await _rolesService.UpdateRoleCustomerRelationAsync(accountId, contactId, isCustomerRelation);
        return Ok();
    }

    /// <summary>
    /// Supprimer le role d'un contact dans une entité morale.
    /// </summary>
    /// <param name="currentUserId">Identifiant du contact à l'origine de l'action.</param>
    /// <param name="accountId">Identifiant de l'entitié morale.</param>
    /// <param name="contactId">Identifiant de l'utilisateur.</param>
    /// <returns>http 200.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> DeleteRoleAsync([FromHeader(Name = "CurrentUser")] int currentUserId, [Required] int accountId, [Required] int contactId)
    {
        await _rolesService.DeleteRoleAsync(currentUserId, accountId, contactId);
        return Ok();
    }
}
