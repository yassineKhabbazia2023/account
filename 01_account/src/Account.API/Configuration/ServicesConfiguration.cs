// <copyright file="ServicesConfiguration.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Services;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Back.Events;

namespace Pulse.Account.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IDelegationService, DelegationService>();
            services.AddScoped<IDelegationRepository, DelegationRepository>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IReferentialService, ReferentialService>();
            services.AddScoped<IReferentialRepository, ReferentialRepository>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IStatisticsRepository, StatisticsRepository>();
            services.AddScoped<IRoleEventPusblisher, RoleEventPublisher>();
        }

        public static void RegisterBroker(this IServiceCollection services, IConfiguration configuration)
        {
            var topicName = configuration["TopicName"];
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new NullArgumentException(Errors.NotFoundTopicName, Errors.NotFoundTopicName);
            }

            var brokerConnectionString = configuration["ServiceBusConnectionString"];
            if (string.IsNullOrWhiteSpace(brokerConnectionString))
            {
                throw new NullArgumentException(Errors.NotFoundServiceBusConnectionString, Errors.NotFoundServiceBusConnectionStringMessage);
            }

            var options = new BrokerOptions
            {
                ServiceBusConnectionString = brokerConnectionString,
                PushTopicName = topicName
            };

            services.AddEventPushServices(options);
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
            var applicationInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            ArgumentNullException.ThrowIfNullOrEmpty(applicationInsightsConnectionString);
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = applicationInsightsConnectionString;
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
