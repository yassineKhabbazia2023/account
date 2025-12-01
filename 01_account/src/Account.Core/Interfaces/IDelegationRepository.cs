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

    Task<IEnumerable<CreateRoleRequest>> CreateDelegationAsync(int contactId, CreateDelegationRequest delegation, IEnumerable<CreateRoleRequest> roles);

    Task<IEnumerable<Role>> DeleteDelegationAsync(int delegationId, int delegatorId, int delegateeId);

    Task<Paging<Delegation>> GetAccountDelegationsHistoryAsync(int accountId, string? search, Pagination pagination);

    Task<bool> DoesAccountExistAsync(int accountId);

    Task<bool> DoesContactExistAsync(int contactId);

    Task<IEnumerable<int>> GetAccountIdsForFullDelegationAsync(int delegatorId);

    Task<Paging<Delegation>> GetContactDelegationsHistoryAsync(int contactId, Pagination pagination, bool sortAscending);

    Task<bool> IsClient(IEnumerable<int> contactIds);

    Task<bool> CanBeDeleted(int currentUserId, int delegationId);
}
