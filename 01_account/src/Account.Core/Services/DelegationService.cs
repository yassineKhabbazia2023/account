// <copyright file="DelegationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

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

    public async Task<int> CreateDelegationAsync(CreateDelegation delegation)
    {
        return await _delegationRepository.CreateDelegationAsync(delegation);
    }

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId)
    {
        return await _delegationRepository.GetContactDelegationsAsync(delegateeId);
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId)
    {
        return await _delegationRepository.GetDelegationsAsync(delegatorId, delegateeId);
    }
}
