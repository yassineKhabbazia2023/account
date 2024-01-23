// <copyright file="HealthCheckExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using HealthChecks.UI.Client;
using Kpmg.Account.Core.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Kpmg.Account.API.Configuration
{
    public static class HealthCheckExtension
    {
        public static void ConfigureHealthCheckService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddHealthChecksUI()
                    .AddInMemoryStorage();
            if (configuration != null)
            {
                services.Configure<HealthCheckConfiguration>(configuration.GetSection("HealthCheck"));
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
