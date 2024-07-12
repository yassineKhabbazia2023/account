// <copyright file="IRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IRoleEventRepository
{
    Task<IEnumerable<RoleEntity>> DeleteContactRolesAsync(int contactId);

    Task<IEnumerable<CreateRoleRequest>> CreateRoleForAutomaticDelegations(int delegatorId, int accountIds);

    Task<CreateRoleRequest?> CreateRoleForNewContact(int contactId, string accountNumber);
}
