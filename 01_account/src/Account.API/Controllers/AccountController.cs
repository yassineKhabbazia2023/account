// <copyright file="AccountController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.Model;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
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
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="pagination">Paramètres de pagination.</param>
        /// <returns>Liste d'entités morales.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<AccountModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Paging<AccountModel>>> GetAccountsAsync([FromQuery] SearchAccountCriteria criteria,
            [FromQuery] Pagination? pagination)
        {
            var result = await _accountService.GetAccountsAsync(criteria, pagination);

            return Ok(result);
        }

        /// <summary>
        /// Lister toutes les entités morales de la BD.
        /// </summary>
        /// <param name="accountNumber">AccountNumber de l'entité morale.</param>
        /// <param name="pagination">Paramètres de pagination.</param>
        /// <returns>Liste d'entités morales.</returns>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Paging<AccountModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Paging<AccountModel>>> GetAllAccountsAsync(string? accountNumber,
            [FromQuery] Pagination? pagination)
        {
            var result = await _accountService.GetAllAccountsAsync(accountNumber, pagination);

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
        /// <param name="accountPatch">Informations à mettre à jour.</param>
        /// <returns>Les informations détaillées de l'entité morale mises à jour.</returns>
        [HttpPatch("{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccountAsync(int accountId, [FromBody] JsonPatchDocument<AccountDetail> accountPatch)
        {
            if (accountPatch == null)
            {
                throw new BadRequestException(Errors.BadRequestAccountPatchCode, Errors.BadRequestAccountPatchMessage);
            }

            var accountToUpdate = await _accountService.GetAccountAsync(accountId);
            accountPatch.ApplyTo(accountToUpdate!);
            await _accountService.UpdateAccountAsync(accountId, accountToUpdate!);

            return Ok();
        }

        /// <summary>
        /// Récupérer la liste des contacts d'une entité morale.
        /// </summary>
        /// <param name="accountId">ID de l'entité morale.</param>
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="pagination">Paramètre de pagination.</param>
        /// <returns>La liste des contacts.</returns>
        [HttpGet("{accountId}/contacts")]
        [ProducesResponseType(typeof(IEnumerable<Contact>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Paging<Contact>>> GetContactsAccountAsync(int accountId, [FromQuery] SearchContactsAccountCriteria criteria, [FromQuery] Pagination? pagination)
        {
            var result = await _accountService.GetContactsAccountAsync(accountId, criteria, pagination);
            return Ok(result);
        }

        /// <summary>
        /// Récupérer la liste des contacts rattachés aux entités d'un contact admin.
        /// </summary>
        /// <param name="contactId">Identifiant du contact connecté.</param>
        /// <param name="request">Paramètre de la requête.</param>
        /// <param name="pagination">Paramètres de pagination.</param>
        /// <returns>La liste des contacts rattachés aux entités d'un contact admin.</returns>
        [HttpGet("contacts/{contactId}")]
        [ProducesResponseType(typeof(IEnumerable<Contact>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Anomaly), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Anomaly), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Contact>>> GetAssociatedContactsAsync(int contactId,
            [FromQuery] GetAssociatedContactsRequest request,
            [FromQuery] Pagination? pagination)
        {
            var result = await _accountService.GetAssociatedContactsAsync(contactId, request, pagination);
            return Ok(result);
        }
    }
}
