// <copyright file="IDelegationService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationService
{
    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegateeId);

    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(Guid delegatorId, Guid delegateeId);

    Task<Guid> CreateDelegationAsync(CreateDelegation delegation);
}
