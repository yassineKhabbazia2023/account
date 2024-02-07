// <copyright file="AccountController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Account.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.API.Controllers
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

        [HttpGet("accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Paging<AccountModel>>> GetAccountsAsync(string? search, int contactId, int page, int limit)
        {
            var result = await _accountService.GetAccountsAsync(search, page, limit, contactId);

            return Ok(result);
        }

        [HttpGet("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDetail>> GetAccountDetailAsync(int accountNumber)
        {
            var result = await _accountService.GetAccountDetailAsync(accountNumber);

            return Ok(result);
        }

        [HttpPatch("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDetail>> UpdateAccountAsync(int accountId, [FromBody] AccountDetail accountDetail)
        {
            var result = await _accountService.UpdateAccountAsync(accountId, accountDetail);

            return Ok(result);
        }

        [HttpGet("favorites/{contactId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountFavorite>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IReadOnlyCollection<AccountFavorite>> GetAccountFavoritesAsync(int contactId)
        {
            var result = _accountService.GetAccountFavoritesAsync(contactId);

            return Ok(result);
        }

        [HttpPatch("favorites/{accountId}/{contactId}/{isFavorite}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult SetFavoriteAsync(int accountId, int contactId, bool isFavorite)
        {
            _accountService.SetFavoriteAsync(accountId, contactId, isFavorite);

            return Ok();
        }
    }
}
