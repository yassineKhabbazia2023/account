// <copyright file="DelegationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Broker.Events;
using Pulse.Account.Core.Broker.Events.DataEvents;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services;

public class DelegationService : IDelegationService
{
    private readonly IDelegationRepository _delegationRepository;
    private readonly IServicePublisher _servicePublisher;
    private readonly ILogger<DelegationService> _logger;

    public DelegationService(
        IDelegationRepository delegationRepository,
        IServicePublisher servicePublisher,
        ILogger<DelegationService> logger)
    {
        _delegationRepository = delegationRepository;
        _servicePublisher = servicePublisher;
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
        var roleIds = await _delegationRepository.CreateDelegationAsync(delegation, roles);

        if (roles.Any())
        {
            for (int i = 0; i < roles.Count(); i++)
            {
                await SendCreatedRoleEvent(roleIds.ElementAt(i), roles.ElementAt(i));
            }
        }
    }

    public async Task DeleteDelegationAsync(int delegationId)
    {
        if (delegationId <= 0)
        {
            throw new BadRequestException(Errors.BadRequestDeleteDelegationCode, Errors.BadRequestDeleteDelegationMessage);
        }

        await _delegationRepository.DeleteDelegationAsync(delegationId);
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

    private async Task SendCreatedRoleEvent(int roleId, CreateRoleRequest role)
    {
        _logger.LogInformation("DelegationService: Start send create role event. Id : {roleId}", roleId);

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

        _logger.LogInformation("DelegationService: End send create role event. Id : {roleId}", roleId);
    }
}
