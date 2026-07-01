// <copyright file="IProspectConversionService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IProspectConversionService
{
    /// <summary>
    /// On reception of a CLIENT <see cref="RegistryAccountCreatedEvent"/>, detects an active prospect sharing the
    /// same SIRET and deactivates it (account soft-delete + roles hard-delete), under feature flag.
    /// </summary>
    Task HandleClientCreatedAsync(RegistryAccountStateEventData clientEvent);
}
