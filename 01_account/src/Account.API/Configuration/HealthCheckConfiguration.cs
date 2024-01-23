// <copyright file="HealthCheckExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Pulse.Account.API.Configuration
{
    public static class HealthCheckConfiguration
    {
        public static void ConfigureHealthCheckService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddHealthChecksUI()
                    .AddInMemoryStorage();
            if (configuration != null)
            {
                services.Configure<HealthChecks.UI.Data.HealthCheckConfiguration>(configuration.GetSection("HealthCheck"));
            }
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
