// <copyright file="RolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Back.ExceptionMiddleware.BaseException;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class RolesService : IRolesService
{
    private readonly IRoleRepository _rolesRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;
    private readonly IHistoryEventPublisher _historyEventPublisher;
    private readonly IRoleLabelService _roleLabelService;
    private readonly ILogger<RolesService> _logger;
    private readonly IFeatureFlagService _featureFlagService;

    public RolesService(IRoleRepository rolesRepository,
        IContactRepository contactRepository,
        IRoleEventPublisher roleEventPublisher,
        IHistoryEventPublisher historyEventPublisher,
        IRoleLabelService roleLabelService,
        ILogger<RolesService> logger,
        IFeatureFlagService featureFlagService)
    {
        _rolesRepository = rolesRepository;
        _contactRepository = contactRepository;
        _roleEventPublisher = roleEventPublisher;
        _historyEventPublisher = historyEventPublisher;
        _roleLabelService = roleLabelService;
        _logger = logger;
        _featureFlagService = featureFlagService;
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

            await PublishRoleCreatedEvent(roleToPublish, includeProspects: true);
            await PublishHistoryCreatedEvent(currentUserId, roleCreated.ContactId, roleCreated.AccountId, actionCode);
        }
    }

    private async Task CreateRoleForProspectAsync(CreateRoleRequest role, int currentUserId)
    {
        var roleCreated = await _rolesRepository.CreateRoleWithoutAccountValidationAsync(role);

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

            await PublishRoleCreatedEvent(roleToPublish, includeProspects: true);
        }
    }

    public async Task<CreateRolesBulkResult> CreateRolesBulkAsync(int accountId, CreateRolesBulkRequest request, int currentUserId)
    {
        var result = new CreateRolesBulkResult();

        if (request?.Contacts == null || request.Contacts.Count == 0)
        {
            return result;
        }

        foreach (var item in request.Contacts)
        {
            try
            {
                var contact = await _contactRepository.GetContactByIdAsync(item.ContactId);
                var role = new CreateRoleRequest
                {
                    AccountId = accountId,
                    ContactId = item.ContactId,
                    IsSignatory = item.IsSignatory,
                    IsFavorite = item.IsFavorite,
                    IsDelegation = item.IsDelegation,
                    IncludePennylaneAccess = item.IncludePennylaneAccess,
                    ContactFlagPortailFactures = item.ContactFlagPortailFactures,
                    IsCustomerRelation = ContactType.Collaborator.ToString().Equals(contact.Type) ? true : null,
                };

                await CreateRoleForProspectAsync(role, currentUserId);

                try
                {
                    await _roleLabelService.AssignRoleLabelFromCodeAsync(item.RoleCode, accountId, item.ContactId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bulk: échec de l'ajout du label {RoleCode} pour AccountId: {AccountId} - ContactId: {ContactId}", item.RoleCode, accountId, item.ContactId);
                }

                result.Succeeded.Add(new CreateRolesBulkItemResult { ContactId = item.ContactId });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "RoleService: Bulk create failed for contact {contactId} on account {accountId}", item.ContactId, accountId);
                result.Failed.Add(new CreateRolesBulkItemResult
                {
                    ContactId = item.ContactId,
                    ErrorCode = ex.Code,
                    ErrorMessage = ex.Message,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Account role assignment failed unexpectedly. ProspectCreationStep: {ProspectCreationStep}, ServiceName: {ServiceName}, OperationName: {OperationName}, AccountId: {AccountId}, ContactId: {ContactId}",
                    "AssignAccountRolesAsync",
                    "Pulse.Back.Account",
                    nameof(CreateRolesBulkAsync),
                    accountId,
                    item.ContactId);
                throw;
            }
        }

        return result;
    }

    public async Task UpdateRoleSignatoryAsync(int currentUserId, int accountId, int contactId, bool isSignatory)
    {
        await _rolesRepository.UpdateRoleSignatoryAsync(accountId, contactId, isSignatory);

        var actionCode = isSignatory ? ActionCode.ADDSIGNMANU.ToString() : ActionCode.DELSIGNMANU.ToString();

        await PublishRoleUpdatedEvent(accountId, contactId);
        await PublishHistoryCreatedEvent(currentUserId, contactId, accountId, actionCode);
    }

    public async Task UpdateRoleCustomerRelationAsync(int accountId, int contactId, bool isCustomerRelation)
    {
        var actionLevel = isCustomerRelation ? (int)ActionLevelType.DirectClientRelation : (int)ActionLevelType.Observator;

        await _rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, isCustomerRelation, actionLevel);

        await PublishRoleUpdatedEvent(accountId, contactId);
    }

    public async Task DeleteRoleAsync(int currentUserId, int accountId, int contactId)
    {
        var role = await _rolesRepository.GetContactRoleAsync(accountId, contactId)
            ?? throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));

        if (role.IsSignatory.HasValue && role.IsSignatory.Value)
        {
            if (await _rolesRepository.IsProspectAccountAsync(accountId))
            {
                throw new BadRequestException(Errors.CannotDeleteSignatoryProspectCode, Errors.CannotDeleteSignatoryProspectMessage);
            }

            var signatory = await _rolesRepository.GetSignatoryAsync(accountId);
            if (signatory.Count() == 1)
            {
                throw new BadRequestException(Errors.CannotDeleteSignatoryCode, Errors.CannotDeleteSignatoryMessage);
            }
        }

        if (await _rolesRepository.IsProspectAccountAsync(accountId) && await _roleLabelService.HasExclusiveLabelAsync(accountId, contactId))
        {
            throw new BadRequestException(Errors.CannotDeleteRoleWithExclusiveLabelCode, Errors.CannotDeleteRoleWithExclusiveLabelMessage);
        }

        await _rolesRepository.DeleteRoleAsync(accountId, contactId);

        var contact = await _contactRepository.GetContactByIdAsync(contactId);
        var actionCode = ContactType.Collaborator.ToString().Equals(contact.Type) ? ActionCode.DELKMANU.ToString() : ActionCode.DELCMANU.ToString();

        await PublishRoleDeletedEvent(accountId, contactId);
        await PublishHistoryCreatedEvent(currentUserId, contactId, accountId, actionCode);
    }

    public async Task<bool> CheckRoleExistsAsync(int currentUserId, int? contactId, int? accountId, string? email)
    {
        var includeProspects = await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.IncludeProspectsInContactsSearch);
        return await _rolesRepository.CheckRoleExistsAsync(currentUserId, contactId, accountId, email, includeProspects);
    }

    public async Task<bool> IsContactHasRoleOnAccount(int contactId, int? accountId, string? accountNumber)
    {
        return await _rolesRepository.IsContactHasRoleOnAccount(contactId, accountId, accountNumber);
    }

    private async Task PublishRoleCreatedEvent(CreateRoleRequest role, bool includeProspects = false)
    {
        _logger.LogInformation("RoleService: Start send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);

        await _roleEventPublisher.PublishRoleCreatedEventAsync(role, includeProspects: includeProspects);

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

        await _roleEventPublisher.PublishRoleDeletedEventAsync(accountId, contactId, includeProspects: true);

        _logger.LogInformation("RoleService: End send delete role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
    }

    private async Task PublishHistoryCreatedEvent(int currentUserId, int contactId, int accountId, string actionCode)
    {
        _logger.LogInformation("RoleService: Start send history created event. CurrentUserId: {currentUserId} AccountId : {accountId} - ContactId : {contactId}", currentUserId, accountId, contactId);

        await _historyEventPublisher.PublishHistoryCreatedEventAsync(currentUserId, contactId, accountId, actionCode);

        _logger.LogInformation("RoleService: End send history created event. CurrentUserId: {currentUserId} AccountId : {accountId} - ContactId : {contactId}", currentUserId, accountId, contactId);
    }
}
