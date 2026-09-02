// <copyright file="IDelegationRequestRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRequestRepository
{
    Task CreateDelegationRequestsAsync(int requesterId, int accountId, int[] recipientIds, string status);

    Task<Paging<DelegationRequest>> GetSentRequestsAsync(int contactId, string? status, Pagination pagination);

    Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, string[]? statuses, Pagination pagination);

    Task<bool> HasRoleOnAccountAsync(int contactId, int accountId);

    Task<bool> HasActiveDelegationOnAccountAsync(int contactId, int accountId);

    Task<bool> HasPendingRequestAsync(int requesterId, int accountId);

    Task<bool> DoesAccountExistAsync(int accountId);

    Task<bool> DoesContactExistAsync(int contactId);

    Task<List<DelegationRequest>> GetPendingRequestsByIdsAndRecipientAsync(int[] delegationRequestIds, int recipientId);

    Task<List<DelegationRequest>> GetAcceptedSiblingRequestsAsync(int requesterId, int accountId, DateTime respondedAt);

    Task AcceptRequestsAsync(int[] delegationRequestIds, DateTime respondedAt);

    Task RefuseRequestsAsync(int[] delegationRequestIds, DateTime respondedAt);

    Task AcceptSiblingRequestsAsync(int requesterId, int accountId, DateTime respondedAt);

    Task<bool> AreAllSiblingRequestsRefusedAsync(int requesterId, int accountId);
}