// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class RolesService : IRolesService
{
    private readonly IRoleRepository _rolesRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly ILogger<RolesService> _logger;

    public RolesService(IRoleRepository rolesRepository,
        IContactRepository contactRepository,
        IRoleEventPublisher roleEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        ILogger<RolesService> logger)
    {
        _rolesRepository = rolesRepository;
        _contactRepository = contactRepository;
        _roleEventPublisher = roleEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _logger = logger;
    }

    public async Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, Pagination? pagination)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        return await _rolesRepository.GetContactRolesAsync(contactId, pagination);
    }

    public async Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId)
    {
        return await _rolesRepository.GetSignatoryAsync(accountId);
    }

    public async Task CreateRoleAsync(CreateRoleRequest role, int currentUserId)
    {
        var roleCreated = await _rolesRepository.CreateRoleAsync(role);

        if (roleCreated != null)
        {
            var roleToPublish = new CreateRoleRequest
            {
                ContactId = roleCreated.ContactId,
                AccountId = roleCreated.AccountId,
                IsSignatory = roleCreated.IsSignatory,
                IsFavorite = roleCreated.IsFavorite,
                IsDelegation = roleCreated.IsDelegation,
                DelegatorId = currentUserId
            };

            var contact = await _contactRepository.GetContactByIdAsync(roleCreated.ContactId);
            var actionCode = ContactType.Collaborator.ToString().Equals(contact.Type) ? ActionCode.ADDKMANU.ToString() : ActionCode.ADDCMANU.ToString();

            await PublishRoleCreatedEvent(roleToPublish);
            await PublishHistoryCreatedEvent(currentUserId, roleCreated.ContactId, roleCreated.AccountId, actionCode);
        }
    }

    public async Task UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        var role = await _rolesRepository.UpdateRoleSignatoryAsync(accountId, contactId, isSignatory);

        if (role == null)
        {
            throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleCode, contactId, accountId));
        }

        await PublishRoleUpdatedEvent(accountId, contactId);
    }

    public async Task UpdateRoleCustomerRelationAsync(int accountId, int contactId, bool isCustomerRelation)
    {
        var actionLevel = isCustomerRelation ? (int)ActionLevelType.DirectClientRelation : (int)ActionLevelType.Observator;

        await _rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, isCustomerRelation, actionLevel);

        await PublishRoleUpdatedEvent(accountId, contactId);
    }

    public async Task DeleteRoleAsync(int currentUserId, int accountId, int contactId)
    {
        var role = await _rolesRepository.GetContactRoleAsync(accountId, contactId);

        if (role == null)
        {
            throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
        }

        if (role.IsSignatory.HasValue && role.IsSignatory.Value)
        {
            var signatory = await _rolesRepository.GetSignatoryAsync(accountId);
            if (signatory.Count() == 1)
            {
                throw new BadRequestException(Errors.CannotDeleteSignatoryCode, Errors.CannotDeleteSignatoryMessage);
            }
        }

        await _rolesRepository.DeleteRoleAsync(accountId, contactId);

        var contact = await _contactRepository.GetContactByIdAsync(contactId);
        var actionCode = ContactType.Collaborator.ToString().Equals(contact.Type) ? ActionCode.DELKMANU.ToString() : ActionCode.DELCMANU.ToString();

        await PublishRoleDeletedEvent(accountId, contactId);
        await PublishHistoryCreatedEvent(currentUserId, contactId, accountId, actionCode);
    }

    public async Task<bool> CheckRoleExistsAsync(int currentUserId, int? contactId, int? accountId, string? email)
    {
        return await _rolesRepository.CheckRoleExistsAsync(currentUserId, contactId, accountId, email);
    }

    public async Task<bool> IsContactHasRoleOnAccount(int contactId, int? accountId, string? accountNumber)
    {
        return await _rolesRepository.IsContactHasRoleOnAccount(contactId, accountId, accountNumber);
    }

    private async Task PublishRoleCreatedEvent(CreateRoleRequest role)
    {
        _logger.LogInformation("RoleService: Start send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);

        await _roleEventPublisher.PublishRoleCreatedEventAsync(role);

        _logger.LogInformation("RoleService: End send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);
    }

    private async Task PublishRoleUpdatedEvent(int accountId, int contactId)
    {
        _logger.LogInformation("RoleService: Start send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);

        await _roleEventPublisher.PublishRoleUpdatedEventAsync(accountId, contactId);

        _logger.LogInformation("RoleService: End send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
    }

    private async Task PublishRoleDeletedEvent(int accountId, int contactId)
    {
        _logger.LogInformation("RoleService: Start send delete role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);

        await _roleEventPublisher.PublishRoleDeletedEventAsync(accountId, contactId);

        _logger.LogInformation("RoleService: End send delete role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
    }

    private async Task PublishHistoryCreatedEvent(int currentUserId, int contactId, int accountId, string actionCode)
    {
        _logger.LogInformation("RoleService: Start send history created event. CurrentUserId: {currentUserId} AccountId : {accountId} - ContactId : {contactId}", currentUserId, accountId, contactId);

        await _historyEventPublisher.PublishHistoryCreatedEventAsync(currentUserId, contactId, accountId, actionCode);

        _logger.LogInformation("RoleService: End send history created event. CurrentUserId: {currentUserId} AccountId : {accountId} - ContactId : {contactId}", currentUserId, accountId, contactId);
    }
}
