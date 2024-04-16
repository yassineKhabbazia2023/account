// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Extensions.Logging;
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
    private readonly IRoleEventPusblisher _roleEventPublisher;
    private readonly ILogger<RolesService> _logger;

    public RolesService(IRoleRepository rolesRepository, IRoleEventPusblisher roleEventPublisher, ILogger<RolesService> logger)
    {
        _rolesRepository = rolesRepository;
        _roleEventPublisher = roleEventPublisher;
        _logger = logger;
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

        await PublishRoleCreatedEvent(role);
    }

    public async Task UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        var role = await _rolesRepository.UpdateRoleSignatoryAsync(accountId, contactId, isSignatory);

        ArgumentNullException.ThrowIfNull(role);

        await PublishRoleUpdatedEvent(accountId, contactId, isSignatory);
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

        await PublishRoleDeletedEvent(accountId, contactId);
    }

    private async Task PublishRoleCreatedEvent(CreateRoleRequest role)
    {
        _logger.LogInformation("RoleService: Start send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);

        await _roleEventPublisher.PublishRoleCreatedEventAsync(role);

        _logger.LogInformation("RoleService: End send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);
    }

    private async Task PublishRoleUpdatedEvent(int accountId, int contactId, bool isSignatory)
    {
        _logger.LogInformation("RoleService: Start send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);

        await _roleEventPublisher.PublishRoleUpdatedEventAsync(accountId, contactId, isSignatory);

        _logger.LogInformation("RoleService: End send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
    }

    private async Task PublishRoleDeletedEvent(int accountId, int contactId)
    {
        _logger.LogInformation("RoleService: Start send delete role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);

        await _roleEventPublisher.PublishRoleDeletedEventAsync(accountId, contactId);

        _logger.LogInformation("RoleService: End send delete role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
    }
}
