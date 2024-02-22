// <copyright file="ReferentialController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.API.Controllers
{
    [Route("api/referentials")]
    [ApiController]
    public class ReferentialController : ControllerBase
    {
        private readonly IReferentialService _referentialService;

        public ReferentialController(IReferentialService referentialService)
        {
            _referentialService = referentialService;
        }

        /// <summary>
        /// Récupère la liste des hubs.
        /// </summary>
        /// <returns>Liste de hubs.</returns>
        [HttpGet("hubs")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Hub?>))]
        public async Task<ActionResult<IReadOnlyCollection<Hub?>>> GetHubsAsync()
        {
            var result = await _referentialService.GetHubsAsync();

            return Ok(result);
        }
    }
}
