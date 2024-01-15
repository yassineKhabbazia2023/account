// <copyright file="HealthCheckExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using HealthChecks.UI.Client;
using Kpmg.Offer.Core.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Kpmg.Offer.API.Configuration
{
    public static class HealthCheckExtension
    {
        public static void ConfigureHealthCheckService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddHealthChecksUI()
                    .AddInMemoryStorage();
            services.Configure<HealthCheckConfiguration>(configuration.GetSection("HealthCheck"));
        }

        public static void UseHealthcheckUI(IApplicationBuilder app)
        {
            app.UseHealthChecks("/api/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(config =>
            {
                config.UIPath = "/dashboard";
            });
        }
    }
}
