// <copyright file="IDelegationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRepository
{
    Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId);

    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId);

    Task CreateDelegationAsync(CreateDelegationRequest delegation, IEnumerable<CreateRoleRequest> roles);

    Task<IEnumerable<Role>> DeleteDelegationAsync(int delegationId);

    Task<Paging<Delegation>> GetAccountDelegationsHistoryAsync(int accountId, string? search, Pagination pagination);

    Task<bool> DoesAccountExistAsync(int accountId);
}
