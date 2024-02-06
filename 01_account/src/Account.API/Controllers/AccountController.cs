// <copyright file="AccountController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService entityService)
        {
            _accountService = entityService;
        }

        /// <summary>
        /// Recherche des entités morales.
        /// </summary>
        /// <param name="search">Critère de recherche (nom/n° IBS de l'entité).</param>
        /// <param name="page">Numéro de page.</param>
        /// <param name="limit">Nombre d'éléments par page.</param>
        /// <returns>Liste d'entités morales.</returns>
        [HttpGet("accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit)
        {
            var result = _accountService.GetAccountsAsync(search, page, limit);

            return Ok(result);
        }

        /// <summary>
        /// Récupérer les informations détaillées d'une entité morale.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <returns>Informations détaillées de l'entité morale.</returns>
        [HttpGet("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDetail> GetAccountDetailAsync(Guid accountId)
        {
            var result = _accountService.GetAccountDetailAsync(accountId);

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour partiellement les informations d'une entité morale.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <param name="accountDetail">Informations à mettre à jour.</param>
        /// <returns>Les informations détaillées de l'entité morale mises à jour.</returns>
        [HttpPatch("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDetail> UpdateAccountAsync(Guid accountId, [FromBody] AccountDetail accountDetail)
        {
            var result = _accountService.UpdateAccountAsync(accountId, accountDetail);

            return Ok(result);
        }

        /// <summary>
        /// Lister les entités morales favorites d'un contact.
        /// </summary>
        /// <param name="contactId">ID du contact.</param>
        /// <returns>Liste des entités morales favorites.</returns>
        [HttpGet("favorites/{contactId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountFavorite>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IReadOnlyCollection<AccountFavorite>> GetAccountFavoritesAsync(Guid contactId)
        {
            var result = _accountService.GetAccountFavoritesAsync(contactId);

            return Ok(result);
        }

        /// <summary>
        /// Modifier le statut de favori d'une entité morale pour un contact donné.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <param name="contactId">ID du contact.</param>
        /// <param name="isFavorite">True si le l'entité morale fait parti des favoris, false sinon.</param>
        /// <returns>OK si la mise à jour s'est bien déroulée.</returns>
        [HttpPatch("favorites/{accountId}/{contactId}/{isFavorite}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult SetFavoriteAsync(Guid accountId, Guid contactId, bool isFavorite)
        {
            _accountService.SetFavoriteAsync(accountId, contactId, isFavorite);

            return Ok();
        }
    }
}
