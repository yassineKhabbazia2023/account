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

    Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, string? status, Pagination pagination);

    Task<bool> HasRequesterAccessToAccountAsync(int contactId, int accountId);

    Task<bool> HasRecipientAccessToAccountAsync(int recipientId, int accountId);

    Task<bool> HasPendingRequestAsync(int requesterId, int accountId);

    Task<bool> DoesAccountExistAsync(int accountId);

    Task<bool> DoesContactExistAsync(int contactId);
}