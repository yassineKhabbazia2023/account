// <copyright file="DelegationService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

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

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(Guid delegateeId)
    {
        return await _delegationRepository.GetContactDelegationsAsync(delegateeId);
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegatorId, Guid delegateeId)
    {
        return await _delegationRepository.GetDelegationsAsync(delegatorId, delegateeId);
    }
}
