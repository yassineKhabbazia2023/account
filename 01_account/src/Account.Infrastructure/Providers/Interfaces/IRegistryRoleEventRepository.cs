// <copyright file="IRegistryRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IRegistryRoleEventRepository
{
    public Task<CreateRoleRequest> CreateRoleAsync(RegistryRoleCreatedEventData eventData, int? accountId, int? contactId, bool? isCustomerRelation);

    public Task<bool> RemoveRoleAsync(int accountId, int contactId);

    public Task<int> GetAccountIdByGuidAsync(Guid accountId);

    public Task<ContactEntity> GetContactByGuidAsync(Guid contactId);

    public Task CheckExistingAccountAndContactAsync(int accountId, int contactId);

    Task<CreateRoleRequest?> UpdateRoleContactFlagPortailFacturesAsync(int accountId, int contactId, bool? contactFlagPortailFactures);

    Task<bool> UpdateRoleIsSignatoryAsync(int accountId, int contactId, bool? isSignatory);

    Task<CreateRoleRequest?> UpdateRoleIsCustomerRelationAsync(int accountId, int contactId, bool isCustomerRelation, int actionLevel);
}
