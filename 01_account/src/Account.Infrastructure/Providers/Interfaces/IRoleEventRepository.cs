// <copyright file="IRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IRoleEventRepository
{
    Task<IEnumerable<RoleEntity>> DeleteContactRolesAsync(int contactId);

    Task<IEnumerable<CreateRoleRequest>> CreateRoleForAutomaticDelegationsAsync(int delegatorId, int accountId);

    Task<CreateRoleRequest?> CreateRoleForNewContactAsync(int contactId, int accountId);

    Task<bool> DoesRoleExistAsync(int contactId, int accountId);
}
