// <copyright file="ServicesConfiguration.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Kpmg.Account.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Services;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulses.Account.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IDelegationService, DelegationService>();
            services.AddScoped<IDelegationRepository, DelegationRepository>();
        }

        public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            var connectionString = configuration["SqlAccountConnectionString"];
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);
            services.AddDbContextPool<AccountContext>(options =>
            {
                options.UseSqlServer(connectionString, opt =>
                {
                    opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });

            services.AddHealthChecks()
                .AddSqlServer(connectionString, healthQuery: "SELECT 1;");
        }

        public static void RegisterApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            var applicationInsightsConexionString = configuration["AccountApplicationInsightConnectionString"];

            ArgumentNullException.ThrowIfNullOrEmpty(applicationInsightsConexionString);
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = applicationInsightsConexionString;
            });
        }

        public static void RegisterCors(this IServiceCollection services)
        {
            services.AddCors(options =>
                {
                    options.AddPolicy(
                        name: "CorsPolicy",
                        builder =>
                        {
                            builder.AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials()
                                    .SetIsOriginAllowed(_ => true)
                                    .WithExposedHeaders("content-range", "content-type", "accept-ranges", "link");
                        });
                });
        }
    }
}
