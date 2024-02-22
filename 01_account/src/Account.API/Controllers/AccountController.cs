// <copyright file="AccountController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.Model;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.API.Controllers
{
    [Route("api/accounts")]
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
        /// <param name="pageNumber">Numéro de page.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <param name="contactId">Identification de l'utilisateur connecté.</param>
        /// <returns>Liste d'entités morales.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<AccountModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Paging<AccountModel>>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId)
        {
            var result = await _accountService.GetAccountsAsync(search, pageNumber, pageSize, contactId);

            return Ok(result);
        }

        /// <summary>
        /// Récupérer les informations détaillées d'une entité morale.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <returns>Informations détaillées de l'entité morale.</returns>
        [HttpGet("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDetail>> GetAccountDetailAsync(int accountId)
        {
            var result = await _accountService.GetAccountDetailAsync(accountId);

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour partiellement les informations d'une entité morale.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <param name="accountDetail">Informations à mettre à jour.</param>
        /// <returns>Les informations détaillées de l'entité morale mises à jour.</returns>
        [HttpPatch("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDetail>> UpdateAccountAsync(int accountId, [FromBody] AccountDetail accountDetail)
        {
            var result = await _accountService.UpdateAccountAsync(accountId, accountDetail);

            return Ok(result);
        }

        /// <summary>
        /// Récupérer la liste des contacts d'une entité morale.
        /// </summary>
        /// <param name="accountId">Identifiant de l'entité morale.</param>
        /// <returns>La liste des contacts.</returns>
        [HttpGet("/{accountId}/contacts")]
        [ProducesResponseType(typeof(IEnumerable<Contact>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContactsAccountAsync(int accountId)
        {
            var result = await _accountService.GetContactsAccountAsync(accountId);
            return Ok(result);
        }
    }
}
