// <copyright file="FavoriteController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Account.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Lister les entités morales favorites d'un contact.
        /// </summary>
        /// <param name="contactId">ID du contact.</param>
        /// <returns>Liste des entités morales favorites.</returns>
        [HttpGet("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountFavorite>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<IReadOnlyCollection<AccountFavorite>>> GetAccountFavoritesByContactIdAsync([Required] int contactId)
        {
            var result = await _favoriteService.GetAccountFavoritesByContactIdAsync(contactId);

            return Ok(result);
        }

        /// <summary>
        /// Modifier le statut de favori d'une entité morale pour un contact donné.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <param name="contactId">ID du contact.</param>
        /// <param name="isFavorite">True si le l'entité morale fait parti des favoris, false sinon.</param>
        /// <returns>OK si la mise à jour s'est bien déroulée.</returns>
        [HttpPatch("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult> SetFavoriteAsync([Required] int accountId, [Required] int contactId, [Required] bool isFavorite)
        {
            await _favoriteService.SetFavoriteAsync(accountId, contactId, isFavorite);

            return Ok();
        }
    }
}
