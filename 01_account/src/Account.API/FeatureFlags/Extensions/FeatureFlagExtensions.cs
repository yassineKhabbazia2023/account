// <copyright file="FeatureFlagExtensions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using ConfigCat.Client;
using OpenFeature;
using OpenFeature.Contrib.ConfigCat;
using OpenFeature.Providers.Memory;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.API.FeatureFlags.Extensions;

[ExcludeFromCodeCoverage]
public static class FeatureFlagExtensions
{
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration)
    {
        var sdkKey = configuration.GetValue<string>("ConfigCat:SdkKey");

        if (!string.IsNullOrEmpty(sdkKey))
        {
            var provider = new ConfigCatProvider(sdkKey, options =>
            {
                options.PollingMode = PollingModes.AutoPoll(pollInterval: TimeSpan.FromSeconds(60));
                options.DataGovernance = DataGovernance.EuOnly;
            });
            Api.Instance.SetProviderAsync(provider).GetAwaiter().GetResult();
        }
        else
        {
            var inMemoryProvider = new InMemoryProvider(BuildInMemoryFlags(configuration));
            Api.Instance.SetProviderAsync(inMemoryProvider).GetAwaiter().GetResult();
        }

        services.AddSingleton<IFeatureClient>(Api.Instance.GetClient());
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();

        return services;
    }

    private static IDictionary<string, Flag> BuildInMemoryFlags(IConfiguration configuration)
    {
        var flags = new Dictionary<string, Flag>();

        foreach (var child in configuration.GetSection("FeatureFlags:Defaults").GetChildren())
        {
            if (bool.TryParse(child.Value, out var boolValue))
            {
                flags[child.Key] = new Flag<bool>(
                    new Dictionary<string, bool> { { "on", true }, { "off", false } },
                    boolValue ? "on" : "off");
            }
        }

        return flags;
    }
}
