// <copyright file="IDelegationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRepository
{
    Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId);

    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId);

    Task<int> CreateDelegationAsync(CreateDelegationRequest delegation);

    Task DeleteDelegationAsync(int delegationId);

    Task<IReadOnlyCollection<Delegation>> GetAccountDelegationsHistoryAsync(int accountId);

    Task<bool> DoesAccountExistAsync(int accountId);
}
