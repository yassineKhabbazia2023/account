// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Net;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services;

public class RolesService : IRolesService
{
    private readonly IRoleRepository _rolesRepository;

    public RolesService(IRoleRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }

    public async Task<Paging<Core.Models.Account>> GetContactRolesAsync(int contactId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber == 0 ? 1 : pageNumber;
        pageSize = pageSize == 0 ? int.MaxValue : pageSize;
        return await _rolesRepository.GetContactRolesAsync(contactId, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId)
    {
        return await _rolesRepository.GetSignatoryAsync(accountId);
    }

    public async Task CreateRoleAsync(CreateRole role)
    {
        await _rolesRepository.CreateRoleAsync(role);
    }
}
