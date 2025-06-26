// <copyright file="StatisticsController.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.ExceptionMiddleware.Model;

namespace Pulse.Account.API.Controllers;

[Route("api")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Récupérer les statistiques des comptes en fonction de leur statut.
    /// </summary>
    /// <param name="contactId">ID du contact.</param>
    /// <returns>Le nombre de comptes par statut.</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Statistics))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<Statistics>> GetStatistics([FromQuery] int contactId)
    {
        var result = await _statisticsService.GetStatisticsAsync(contactId);

        return Ok(result);
    }

    /// <summary>
    /// Récupérer le pourcentage des comptes ayant au moins un contact en relation client.
    /// </summary>
    /// <returns>Le pourcentage des comptes ayant au moins un contact en relation client.</returns>
    [HttpGet("account-customer-relation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(double))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<ActionResult<double>> GetAccountPercentageCustomerRelationAsync()
    {
        var result = await _statisticsService.GetAccountPercentageCustomerRelationAsync();

        return Ok(result);
    }

    /// <summary>
    /// Permet de récupérer dans un fichier Excel le nombre de clients par nombre de dossiers et le nombre de dossiers par nombre de clients.
    /// </summary>
    /// <returns>Le fichier Excel contenant les indicateurs.</returns>
    [HttpGet("account-client-indicators")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(File))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<ActionResult> GetAccountAndClientIndicatorsAsync()
    {
        var result = await _statisticsService.GetAccountAndClientIndicatorsAsync();

        return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AccountContactIndicators.xlsx");
    }
}
