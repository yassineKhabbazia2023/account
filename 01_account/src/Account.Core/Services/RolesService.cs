// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
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

    public async Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, int pageNumber, int pageSize)
    {
        pageNumber = Pagination.GetValidPageNumber(pageNumber);
        pageSize = Pagination.GetValidPageSize(pageSize);
        return await _rolesRepository.GetContactRolesAsync(contactId, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId)
    {
        return await _rolesRepository.GetSignatoryAsync(accountId);
    }

    public async Task CreateRoleAsync(CreateRoleRequest role)
    {
        await _rolesRepository.CreateRoleAsync(role);
    }

    public async Task UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        await _rolesRepository.UpdateRoleSignatoryAsync(accountId, contactId, isSignatory);
    }

    public async Task DeleteRoleAsync(int accountId, int contactId)
    {
        var role = await _rolesRepository.GetContactRoleAsync(accountId, contactId);

        if (role == null)
        {
            throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
        }

        if (role.IsSignatory!.Value)
        {
            var signatory = await _rolesRepository.GetSignatoryAsync(accountId);
            if (signatory.Count() == 1)
            {
                throw new BadRequestException(Errors.CannotDeleteSignatoryCode, Errors.CannotDeleteSignatoryMessage);
            }
        }

        await _rolesRepository.DeleteRoleAsync(accountId, contactId);
    }
}
