// <copyright file="HealthCheckConfiguration.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Pulse.Account.API.Configuration
{
    public static class HealthCheckConfiguration
    {
        public static void UseHealthcheckUI(IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
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
