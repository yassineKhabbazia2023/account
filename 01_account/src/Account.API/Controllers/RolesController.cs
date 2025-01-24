// <copyright file="RolesController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

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
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CheckRoleExists([Required] int contactId, int? accountId, [Required] string email)
    {
        var contactHasRoleOnAccount = await _rolesService.CheckRoleExistsAsync(contactId, accountId, email);

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
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> IsContactHasRoleOnAccount([Required] int contactId, int? accountId, string? accountNumber)
    {
        if(!accountId.HasValue && accountNumber.IsNullOrEmpty())
        {
            throw new BadRequestException(Errors.BadRequestAccountIdAndAccountNumberNullCode, Errors.BadRequestAccountIdAndAccountNumberNullMessage);
        }

        var contactHasRoleOnAccount = await _rolesService.IsContactHasRoleOnAccount(contactId, accountId, accountNumber);

        return Ok(contactHasRoleOnAccount);
    }

    /// <summary>
    /// Créer un role pour un contact dans une entité morale.
    /// </summary>
    /// <param name="role">Objet role qui va lier un contact à une entité morale.</param>
    /// <returns>http 201.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateRoleAsync(CreateRoleRequest role)
    {
        await _rolesService.CreateRoleAsync(role);
        return Created();
    }

    /// <summary>
    /// Modifier un role pour un contact.
    /// </summary>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <param name="contactId">Identifiant de l'utilisateur.</param>
    /// <param name="isSignatory">True si l'utilisateur est signataire, false sinon.</param>
    /// <returns>http 200.</returns>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateRoleSignatoryAsync([Required] int accountId, [Required] int contactId, [Required] bool isSignatory)
    {
        await _rolesService.UpdateRoleSignatoryAsync(accountId, contactId, isSignatory);
        return Ok();
    }

    /// <summary>
    /// Supprimer le role d'un contact dans une entité morale.
    /// </summary>
    /// <param name="accountId">Identifiant de l'entitié morale.</param>
    /// <param name="contactId">Identifiant de l'utilisateur.</param>
    /// <returns>http 200.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteRoleAsync([Required] int accountId, [Required] int contactId)
    {
        await _rolesService.DeleteRoleAsync(accountId, contactId);
        return Ok();
    }
}
