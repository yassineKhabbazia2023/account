// <copyright file="OfferController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Offer.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kpmg.Offer.API.Controllers
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService entityService)
        {
            _accountService = entityService;
        }

        [HttpGet("accounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Core.Models.Offer>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyCollection<Core.Models.Offer>>> GetOffersAsync()
        {
            var result = await _accountService.GetOffersAsync();

            return Ok(result);
        }

        [HttpGet("accounts/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Core.Models.Offer))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Core.Models.Offer>> GetOfferByIdAsync(int offerId)
        {
            var result = await _accountService.GetOfferByIdAsync(offerId);

            return Ok(result);
        }
    }
}
