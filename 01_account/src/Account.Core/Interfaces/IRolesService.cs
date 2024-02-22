// <copyright file="IRolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IRolesService
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, int pageNumber, int pageSize);

    Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId);

    Task CreateRoleAsync(CreateRole role);
}
