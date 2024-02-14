// <copyright file="RolesService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _rolesRepository;

    public RolesService(IRolesRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }

    public Task<Paging<Core.Models.Account>> GetContactRolesAsync(int contactId, int page, int limit)
    {
        page = page == 0 ? 1 : page;
        limit = limit == 0 ? int.MaxValue : limit;
        return _rolesRepository.GetContactRolesAsync(contactId, page, limit);
    }
}
