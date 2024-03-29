// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Broker.Events;
using Pulse.Account.Core.Broker.Events.DataEvents;
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
    private readonly IServicePublisher _servicePublisher;
    private readonly ILogger<RolesService> _logger;

    public RolesService(IRoleRepository rolesRepository, IServicePublisher servicePublisher, ILogger<RolesService> logger)
    {
        _rolesRepository = rolesRepository;
        _servicePublisher = servicePublisher;
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
        var roleId = await _rolesRepository.CreateRoleAsync(role);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(roleId.ToString());

        await SendRoleCreatedEvent(roleId, role);
    }

    public async Task UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        var roleId = await _rolesRepository.UpdateRoleSignatoryAsync(accountId, contactId, isSignatory);

        ArgumentNullException.ThrowIfNullOrWhiteSpace(roleId.ToString());

        await SendRoleUpdatedEvent(roleId, accountId, contactId, isSignatory);
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

        var roleId = await _rolesRepository.DeleteRoleAsync(accountId, contactId);

        await SendRoleDeletedEvent(roleId, accountId, contactId);
    }

    private async Task SendRoleCreatedEvent(int roleId, CreateRoleRequest role)
    {
        _logger.LogInformation("RoleService: Start send create role event. Id : {roleId}", roleId);

        await _servicePublisher.PublishAsync(new RoleCreatedEvent
        {
            EventIdentifier = $"RoleId = '{roleId}'",
            DataEvent = new RoleCreatedDataEvent
            {
                RoleId = roleId,
                AccountId = role.AccountId,
                ContactId = role.ContactId,
                IsDelegation = role.IsDelegation,
                IsFavorite = role.IsFavorite,
                IsSignatory = role.IsSignatory
            },
            Sender = "AccountAPI - CreateRole"
        });

        _logger.LogInformation("RoleService: End send create role event. Id : {roleId}", roleId);
    }

    private async Task SendRoleUpdatedEvent(int roleId, int accountId, int contactId, bool isSignatory)
    {
        _logger.LogInformation("RoleService: Start send update role event. Id : {roleId}", roleId);
        await _servicePublisher.PublishAsync(new RoleUpdatedEvent
        {
            EventIdentifier = $"RoleId = '{roleId}'",
            DataEvent = new RoleUpdatedDataEvent
            {
                RoleId = roleId,
                AccountId = accountId,
                ContactId = contactId,
                IsSignatory = isSignatory
            },
            Sender = "AccountAPI - UpdateRole"
        });
        _logger.LogInformation("RoleService: End send update role event. Id : {roleId}", roleId);
    }

    private async Task SendRoleDeletedEvent(int roleId, int accountId, int contactId)
    {
        _logger.LogInformation("RoleService: Start send delete role event. Id : {roleId}", roleId);

        await _servicePublisher.PublishAsync(new RoleDeletedEvent
        {
            EventIdentifier = $"RoleId = '{roleId}'",
            DataEvent = new RoleDeletedDataEvent
            {
                RoleId = roleId,
                AccountId = accountId,
                ContactId = contactId,
            },
            Sender = "AccountAPI - DeleteRole"
        });

        _logger.LogInformation("RoleService: End send delete role event. Id : {roleId}", roleId);
    }
}
