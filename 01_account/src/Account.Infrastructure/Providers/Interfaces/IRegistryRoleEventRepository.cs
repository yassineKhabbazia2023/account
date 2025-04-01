// <copyright file="IRegistryRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IRegistryRoleEventRepository
{
    public Task<CreateRoleRequest> CreateRoleAsync(RegistryRoleCreatedEventData eventData, int? accountId, int? contactId);

    public Task<bool> RemoveRoleAsync(int accountId, int contactId);

    public Task<(int, int)> GetAccountIdContactIdAsync(Guid accountId, Guid contactId);

    public Task CheckExistingAccountAndContactAsync(int accountId, int contactId);
}
