// <copyright file="DelegationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services;

public class DelegationService : IDelegationService
{
    private readonly IDelegationRepository _delegationRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;
    private readonly ILogger<DelegationService> _logger;

    public DelegationService(
        IDelegationRepository delegationRepository,
        IRoleEventPublisher roleEventPublisher,
        ILogger<DelegationService> logger)
    {
        _delegationRepository = delegationRepository;
        _roleEventPublisher = roleEventPublisher;
        _logger = logger;
    }

    public async Task CreateDelegationAsync(CreateDelegationRequest delegation)
    {
        if (delegation is null)
        {
            throw new BadRequestException(Errors.CreateDelegationCode, Errors.CreateDelegationMessage);
        }

        delegation.DelegationDetails.SetDelegationInformation();

        if (!DelegationValidation.ValidateEndDateDelegation(delegation.DelegationDetails))
        {
            throw new BadRequestException(Errors.DelegationEndDateInvalidCode, Errors.DelegationEndDateInvalidMessage);
        }

        var roles = CreateRoleRequests(delegation);
        await _delegationRepository.CreateDelegationAsync(delegation, roles);

        await Task.WhenAll(roles.Select(PublishCreatedRoleEvent));
    }

    public async Task DeleteDelegationAsync(int delegationId)
    {
        if (delegationId <= 0)
        {
            throw new BadRequestException(Errors.BadRequestDeleteDelegationCode, Errors.BadRequestDeleteDelegationMessage);
        }

        var rolesToDelete = await _delegationRepository.DeleteDelegationAsync(delegationId);

        foreach (var role in rolesToDelete)
        {
            await PublishRoleDeletedEvent(role);
        }
    }

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId)
    {
        return await _delegationRepository.GetContactDelegationsAsync(delegateeId);
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId)
    {
        return await _delegationRepository.GetDelegationsAsync(delegatorId, delegateeId);
    }

    public async Task<IReadOnlyCollection<Delegation>> GetAccountDelegationsHistoryAsync(int accountId)
    {
        if (!await _delegationRepository.DoesAccountExistAsync(accountId))
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return await _delegationRepository.GetAccountDelegationsHistoryAsync(accountId);
    }

    public static IEnumerable<CreateRoleRequest> CreateRoleRequests(CreateDelegationRequest delegationRequest)
    {
        if (delegationRequest?.DelegationDetails?.Any() != true)
        {
            return Enumerable.Empty<CreateRoleRequest>();
        }

        var roleRequests = new List<CreateRoleRequest>();

        foreach (var detail in delegationRequest.DelegationDetails)
        {
            if (detail.IsRoleToCreate)
            {
                foreach (var accountId in delegationRequest.AccountIds)
                {
                    roleRequests.Add(new CreateRoleRequest
                    {
                        AccountId = accountId,
                        ContactId = detail.DelegateeId,
                        IsFavorite = false,
                        IsSignatory = false,
                        IsDelegation = true
                    });
                }
            }
        }

        return roleRequests;
    }

    private async Task PublishCreatedRoleEvent(CreateRoleRequest role)
    {
        _logger.LogInformation("DelegationService: Start send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);

        await _roleEventPublisher.PublishRoleCreatedEventAsync(role);

        _logger.LogInformation("DelegationService: End send create role event. AccountId : {accountId} - ContactId : {contactId}", role.AccountId, role.ContactId);
    }

    private async Task PublishRoleDeletedEvent(Role role)
    {
        _logger.LogInformation("DelegationService: Start send delete role event. AccountId : {accountId}, ContactId : {contactId}", role.AccountId, role.ContactId);

        await _roleEventPublisher.PublishRoleDeletedEventAsync(role.AccountId, role.ContactId);

        _logger.LogInformation("DelegationService: End send delete role event. AccountId : {accountId}, ContactId : {contactId}", role.AccountId, role.ContactId);
    }
}
