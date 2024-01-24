// <copyright file="HealthCheckController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kpmg.Account.API.Controllers
{
    [Route("api")]
    [Controller]
    public class HealthCheckController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthCheckController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet("health")]
        public async Task<IActionResult> CheckHealthAsync()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var json = JsonSerializer.Serialize(report);

            if (report.Status != HealthStatus.Healthy)
            {
                return StatusCode(StatusCodes.Status417ExpectationFailed, json);
            }

            return Ok(json);
        }
    }
}
