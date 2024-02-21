// <copyright file="RolesController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Net;
using Kpmg.ExceptionMiddleware.Model;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

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
    /// <param name="page">Numéro de page.</param>
    /// <param name="limit">Nombre d'éléments par page.</param>
    /// <returns>Liste d'entités morales.</returns>
    [HttpGet("{contactId}")]
    [ProducesResponseType(typeof(Paging<Pulse.Account.Core.Models.Account>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Paging<Core.Models.Account>>> GetContactRolesAsync(int contactId, int page, int limit)
    {
        var result = await _rolesService.GetContactRolesAsync(contactId, page, limit);

        return Ok(result);
    }

    /// <summary>
    /// Récupérer la liste des signataires d'une entité morale.
    /// </summary>
    /// <param name="accountId">Identifiant de l'entité morale.</param>
    /// <returns>La liste des signataires.</returns>
    [HttpGet("signatory/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<Signatory>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Signatory>>> GetSignatoryAsync(int accountId)
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
    public async Task<ActionResult> CreateRoleAsync(Role role)
    {
        await _rolesService.CreateRoleAsync(role);
        return StatusCode(StatusCodes.Status201Created);
    }
}
