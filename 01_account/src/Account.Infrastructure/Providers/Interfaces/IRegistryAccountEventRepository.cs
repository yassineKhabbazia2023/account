// <copyright file="IRegistryAccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IRegistryAccountEventRepository
{
    public Task<AccountDetail> CreateAccountAsync(RegistryAccountCreatedEventData eventData);

    public Task<AccountDetail> UpdateAccountAsync(RegistryAccountUpdatedEventData eventData);

    public Task<int> RemoveAccountAsync(Guid accountGlobalUniqueIdentifier);

    Task<bool> DoesAccountExistAsync(Guid accountGlobalUniqueId);

    Task<AccountDetail?> GetAccountByGuidAsync(Guid accountGlobalUniqueId);
}
