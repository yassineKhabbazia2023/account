// <copyright file="RolesController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Net;
using Kpmg.ExceptionMiddleware.Model;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="pageNumber">Numéro de page.</param>
    /// <param name="pageSize">Nombre d'éléments par page.</param>
    /// <returns>Liste d'entités morales.</returns>
    [HttpGet("{contactId}")]
    [ProducesResponseType(typeof(Paging<Pulse.Account.Core.Models.Account>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Paging<Core.Models.Account>>> GetContactRolesAsync(int contactId, int pageNumber, int pageSize)
    {
        var result = await _rolesService.GetContactRolesAsync(contactId, pageNumber, pageSize);

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
}
