// <copyright file="IDelegationService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationService
{
    Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId);

    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId);

    Task<int> CreateDelegationAsync(CreateDelegation delegation);
}
