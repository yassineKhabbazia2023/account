// <copyright file="DelegationController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Models;

namespace Pulse.Account.API.Controllers;

[ApiController]
[Route("api/delegations")]
public class DelegationController : ControllerBase
{
    [HttpGet("{delegatorId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Delegation>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetContactDelegationsAsync(Guid delegatorId)
    {
        var delegationList = new List<Delegation>();
        return Ok(delegationList!);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Delegation>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetDelegationsAsync([FromQuery] Guid delegatorId, [FromQuery] Guid delegateeId)
    {
        var delegationList = new List<Delegation>();
        return Ok(delegationList!);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDelegationsAsync([FromBody] Delegation delegation)
    {
        return Created();
    }
}
