// <copyright file="IDelegationService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationService
{
    Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId);

    Task<Paging<Delegation>> GetDelegatorDelegationsAsync(int delegatorId, DelegationFilter filter, Pagination? pagination);

    Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId);

    Task CreateDelegationAsync(int contactId, CreateDelegationRequest delegation);

    Task DeleteDelegationAsync(int delegationId);

    Task<Paging<Delegation>> GetAccountDelegationsHistoryAsync(int accountId, string? search, Pagination? pagination);

    Task<Paging<Delegation>> GetContactDelegationsHistoryAsync(int contactId, Pagination? pagination, bool sortAscending);
}
