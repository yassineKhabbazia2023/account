// <copyright file="IRolesRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces;

public interface IRoleRepository
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, int page, int limit);

    Task<IEnumerable<Signatory>> GetSignatoryAsync(int accountId);

    Task<int> CreateRoleAsync(Role role);
}
