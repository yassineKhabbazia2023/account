using Microsoft.AspNetCore.Mvc;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.API.Controllers
{
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
        [HttpGet("statistics/{contactId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Statistics))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Statistics>> GetStatistics(int contactId)
        {
            var result = await _statisticsService.GetStatisticsAsync(contactId);

            return Ok(result);
        }
    }
}
