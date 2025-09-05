// <copyright file="ServicesConfiguration.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.API.Configuration.Model;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Services;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Back.Events;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.Configurations;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.API.Configuration;

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
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<IRoleLabelRepository, RoleLabelRepository>();
        services.AddScoped<IRoleLabelService, RoleLabelService>();
        services.AddScoped<IOfferEligibilityRepository, OfferEligibilityRepository>();
        services.AddScoped<IOfferEligibilityService, OfferEligibilityService>();
    }

    public static void RegisterBrokerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var brokerSettings = configuration!.GetSection("BrokerSetting").Get<BrokerSetting>();

        if (brokerSettings!.PushTopicName == null || !brokerSettings!.PushTopicName.Any())
        {
            throw new NullArgumentException(Errors.NotFoundTopicName, Errors.NotFoundTopicNameMessage);
        }

        if (string.IsNullOrWhiteSpace(brokerSettings!.ServiceBusNamespace))
        {
            throw new NullArgumentException(Errors.NotFoundServiceBusNamespaceCode, Errors.NotFoundServiceBusNamespaceMessage);
        }

        if (string.IsNullOrWhiteSpace(brokerSettings!.ManagedIdentityClientId))
        {
            throw new NullArgumentException(Errors.NotFoundManagedIdentityClientIdCode, Errors.NotFoundManagedIdentityClientIdMessage);
        }

        var options = new BrokerOptions
        {
            ServiceBusNamespace = brokerSettings.ServiceBusNamespace,
            ManagedIdentityClientId = brokerSettings.ManagedIdentityClientId,
            PushTopicNames = brokerSettings.PushTopicName
        };

        if (brokerSettings?.PullTopics?.Any() == true)
        {
            foreach (var topic in brokerSettings.PullTopics)
            {
                options.AddPullTopicItem(topic.TopicName!, topic.Subscriptions!);
            }
        }

        services.AddScoped<IContactEventRepository, ContactEventRepository>();
        services.AddScoped<IRegistryAccountEventRepository, RegistryAccountEventRepository>();
        services.AddScoped<IRegistryRoleEventRepository, RegistryRoleEventRepository>();
        services.AddScoped<IAccountEventRepository, AccountEventRepository>();
        services.AddScoped<IRoleEventRepository, RoleEventRepository>();
        services.AddScoped<IDelegationEventRepository, DelegationEventRepository>();
        services.AddScoped<IOfferEligibilityEventRepository, OfferEligibilityEventRepository>();
        services.AddKeyedScoped<IEventHandler, ContactCreatedEventHandler>(nameof(ContactCreatedEvent));
        services.AddKeyedScoped<IEventHandler, ContactUpdatedEventHandler>(nameof(ContactUpdatedEvent));
        services.AddKeyedScoped<IEventHandler, ContactRemovedEventHandler>(nameof(ContactRemovedEvent));
        services.AddKeyedScoped<IEventHandler, RegistryAccountCreatedEventHandler>(nameof(RegistryAccountCreatedEvent));
        services.AddKeyedScoped<IEventHandler, RegistryAccountUpdatedEventHandler>(nameof(RegistryAccountUpdatedEvent));
        services.AddKeyedScoped<IEventHandler, RegistryAccountRemovedEventHandler>(nameof(RegistryAccountRemovedEvent));
        services.AddKeyedScoped<IEventHandler, RegistryRoleCreatedEventHandler>(nameof(RegistryRoleCreatedEvent));
        services.AddKeyedScoped<IEventHandler, RegistryRoleRemovedEventHandler>(nameof(RegistryRoleRemovedEvent));
        services.AddKeyedScoped<IEventHandler, RoleCreatedEventHandler>(nameof(RoleCreatedEvent));
        services.AddKeyedScoped<IEventHandler, SubscriptionValidatedEventHandler>(nameof(SubscriptionValidatedEvent));
        services.AddKeyedScoped<IEventHandler, ReportCreatedEventHandler>(nameof(ReportCreatedEvent));

        services.AddScoped<IAccountEventPublisher, AccountEventPublisher>();
        services.AddScoped<IRoleEventPublisher, RoleEventPublisher>();
        services.AddScoped<IHistoryEventPublisher, HistoryEventPublisher>();
        services.AddScoped<IOfferActivatedEventPublisher, OfferActivatedEventPublisher>();

        services.AddEventPushServices(options);
        services.AddEventPullServices(options);
    }

    public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["SqlAccountConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new NullArgumentException(Errors.NotFoundDatabaseConnectionStringCode, Errors.NotFoundDataBaseConnectionStringMessage);
        }

        services.AddDbContextPool<AccountContext>(options =>
        {
            options.UseSqlServer(connectionString, opt =>
            {
                opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                opt.EnableRetryOnFailure(GlobalConstants.RETRYCOUNT, TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN), null);
            });
        });

        services.AddHealthChecks()
            .AddSqlServer(connectionString, healthQuery: "SELECT 1;");
    }

    public static void RegisterApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var applicationInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (string.IsNullOrEmpty(applicationInsightsConnectionString))
        {
            throw new NullArgumentException(
                Errors.NullArgumentCode,
                string.Format(Errors.NullArgumentMessage, "APPLICATIONINSIGHTS_CONNECTION_STRING"));
        }

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
