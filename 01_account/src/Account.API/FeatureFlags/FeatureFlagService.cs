// <copyright file="FeatureFlagService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using OpenFeature;
using OpenFeature.Model;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.API.FeatureFlags;

[ExcludeFromCodeCoverage]
public class FeatureFlagService(IFeatureClient featureClient) : IFeatureFlagService
{
    public async Task<bool> IsEnabledAsync(string flagKey, string? userEmail = null, CancellationToken cancellationToken = default)
    {
        var context = BuildEvaluationContext(userEmail);
        return await featureClient.GetBooleanValueAsync(flagKey, false, context, cancellationToken: cancellationToken);
    }

    private static EvaluationContext? BuildEvaluationContext(string? userEmail)
    {
        if (string.IsNullOrEmpty(userEmail))
            return null;

        return EvaluationContext.Builder()
            .SetTargetingKey(userEmail.ToLowerInvariant())
            .Build();
    }
}
