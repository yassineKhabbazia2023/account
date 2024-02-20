// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services;

public class RolesService : IRolesService
{
    private readonly IRoleRepository _rolesRepository;

    public RolesService(IRoleRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }

    public Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, int page, int limit)
    {
        page = page == 0 ? 1 : page;
        limit = limit == 0 ? int.MaxValue : limit;
        return _rolesRepository.GetContactRolesAsync(contactId, page, limit);
    }

    public Task<IEnumerable<Signatory>> GetSignatoryAsync(int accountId)
    {
        return _rolesRepository.GetSignatoryAsync(accountId);
    }
}
