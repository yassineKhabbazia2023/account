// <copyright file="IRolesRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IRoleRepository
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, int pageNumber, int pageSize);

    Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId);

    Task CreateRoleAsync(CreateRoleRequest role);

    Task<Role> UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory);

    Task DeleteRoleAsync(int accountId, int contactId);

    Task<Role> GetContactRoleAsync(int accountId, int contactId);
}
