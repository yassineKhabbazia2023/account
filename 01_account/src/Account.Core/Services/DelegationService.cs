// <copyright file="DelegationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services;

public class DelegationService : IDelegationService
{
    private readonly IDelegationRepository _delegationRepository;

    public DelegationService(IDelegationRepository delegationRepository)
    {
        _delegationRepository = delegationRepository;
    }

    public async Task<int> CreateDelegationAsync(CreateDelegationRequest delegation)
    {
        if (delegation is null)
        {
            throw new BadRequestException(Errors.CreateDelegationCode, Errors.CreateDelegationMessage);
        }

        delegation.DelegationDetails.SetStartDateAndStatus();

        if (!Validation.ValidateEndDateDelegation(delegation.DelegationDetails))
        {
            throw new BadRequestException(Errors.DelegationEndDateInvalidCode, Errors.DelegationEndDateInvalidMessage);
        }

        return await _delegationRepository.CreateDelegationAsync(delegation);
    }

    public async Task DeleteDelegationAsync(int delegationId)
    {
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
}
