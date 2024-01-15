// <copyright file="OfferHealthCheckService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Offer.Core.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Kpmg.Offer.Core.Services
{
    public class OfferHealthCheckService : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HealthCheckConfiguration _configuration;

        public OfferHealthCheckService(IHttpClientFactory httpClientFactory, IOptions<HealthCheckConfiguration> configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.GetAsync(_configuration.ConstellationNotificationsUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, "The API is down");
                }

                return new HealthCheckResult(HealthStatus.Healthy, "The API is up and running");
            }
        }
    }
}
