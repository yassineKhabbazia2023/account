// <copyright file="FavoriteController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public FavoriteController(IAccountService entityService)
        {
            _accountService = entityService;
        }

        /// <summary>
        /// Lister les entités morales favorites d'un contact.
        /// </summary>
        /// <param name="contactId">ID du contact.</param>
        /// <returns>Liste des entités morales favorites.</returns>
        [HttpGet("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountFavorite>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyCollection<AccountFavorite>>> GetAccountFavoritesAsync([Required] int contactId)
        {
            var result = await _accountService.GetAccountFavoritesAsync(contactId);

            return Ok(result);
        }

        /// <summary>
        /// Modifier le statut de favori d'une entité morale pour un contact donné.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <param name="contactId">ID du contact.</param>
        /// <param name="isFavorite">True si le l'entité morale fait parti des favoris, false sinon.</param>
        /// <returns>OK si la mise à jour s'est bien déroulée.</returns>
        [HttpPatch("favorites/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetFavoriteAsync(int accountId, [Required] int contactId, [Required] bool isFavorite)
        {
            await _accountService.SetFavoriteAsync(accountId, contactId, isFavorite);

            return Ok();
        }
    }
}
