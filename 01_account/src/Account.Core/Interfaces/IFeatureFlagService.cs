// <copyright file="IFeatureFlagService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string flagKey, string? userEmail = null, CancellationToken cancellationToken = default);
}
