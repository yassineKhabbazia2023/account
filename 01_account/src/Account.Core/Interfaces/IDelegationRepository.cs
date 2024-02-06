// <copyright file="IDelegationRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRepository
{
    Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(Guid delegateeId);

    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegatorId, Guid delegateeId);

    Task<int> CreateDelegationAsync(CreateDelegation delegation);
}
