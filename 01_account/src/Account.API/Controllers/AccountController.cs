// <copyright file="AccountController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
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

        [HttpGet("accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit)
        {
            var result = _accountService.GetAccountsAsync(search, page, limit);

            return Ok(result);
        }

        [HttpGet("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDetail> GetAccountDetailAsync(Guid accountId)
        {
            var result = _accountService.GetAccountDetailAsync(accountId);

            return Ok(result);
        }

        [HttpPatch("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDetail> UpdateAccountAsync(Guid accountId, [FromBody] AccountDetail accountDetail)
        {
            var result = _accountService.UpdateAccountAsync(accountId, accountDetail);

            return Ok(result);
        }

        [HttpGet("favorites/{contactId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountFavorite>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IReadOnlyCollection<AccountFavorite>> GetAccountFavoritesAsync(Guid contactId)
        {
            var result = _accountService.GetAccountFavoritesAsync(contactId);

            return Ok(result);
        }

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
