// <copyright file="AccountController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Account.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;

namespace Kpmg.Account.API.Controllers
{
    [Route("api")]
    //[Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService entityService)
        {
            _accountService = entityService;
        }

        [HttpGet("accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Account.Core.Models.Account>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Paging<Account.Core.Models.Account>> GetAccountList(string search = "", int page = 1, int limit = int.MaxValue)
        {
            var result = _accountService.GetAccountList(search, page, limit);

            return Ok(result);
        }

        [HttpGet("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDetail> GetAccountDetail(Guid accountId)
        {
            var result = _accountService.GetAccountDetail(accountId);

            return Ok(result);
        }

        [HttpPatch("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountDetail))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AccountDetail> UpdateAccount(Guid accountId, [FromBody] AccountDetail updatedAccount)
        {
            var result = _accountService.UpdateAccount(accountId, updatedAccount);

            return Ok(result);
        }

        [HttpGet("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<AccountFavorite>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<AccountFavorite>> GetAccountFavoriteList()
        {
            string token = "token";
            var result = _accountService.GetAccountFavoriteList(token);

            return Ok(result);
        }

        [HttpPatch("favorite/{accountId}/{isFavorite}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult SetFavorite(Guid accountId, bool isFavorite)
        {
            string token = "token";
            _accountService.SetFavorite(accountId, isFavorite, token);

            return Ok();
        }
    }
}
