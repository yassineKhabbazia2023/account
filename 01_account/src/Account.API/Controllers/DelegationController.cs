// <copyright file="DelegationController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.API.Controllers;

[ApiController]
[Route("api/delegations")]
public class DelegationController : ControllerBase
{
    private readonly IDelegationService _delegationService;

    public DelegationController(IDelegationService delegationService)
    {
        _delegationService = delegationService;
    }

    [HttpGet("{delegateeId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Delegation>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetContactDelegationsAsync(Guid delegateeId)
    {
        if (delegateeId == Guid.Empty)
        {
            return BadRequest("The delegateeId parameters are required.");
        }

        var delegationList = await _delegationService.GetContactDelegationsAsync(delegateeId);
        return Ok(delegationList!);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<Delegation>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<Delegation>>> GetDelegationsAsync([FromQuery] Guid delegatorId, [FromQuery] Guid delegateeId)
    {
        if (delegatorId == Guid.Empty || delegateeId == Guid.Empty)
        {
            return BadRequest("The delegatorId and delegateeId parameters are required.");
        }

        var delegationList = await _delegationService.GetDelegationsAsync(delegatorId, delegateeId);
        return Ok(delegationList!);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateDelegationAsync([FromBody] CreateDelegation delegation)
    {
        if (delegation is null)
        {
            return BadRequest("The delegation parameter are required.");
        }

        int delegationId = await _delegationService.CreateDelegationAsync(delegation);

        return Ok(delegationId);
    }
}
