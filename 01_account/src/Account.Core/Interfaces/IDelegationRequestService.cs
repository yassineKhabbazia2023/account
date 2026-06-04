// <copyright file="IDelegationRequestService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRequestService
{
    Task<CreateDelegationRequestsResponse> CreateDelegationRequestsAsync(int contactId, CreateDelegationRequestsRequest request);

    Task<Paging<DelegationRequest>> GetSentRequestsAsync(int contactId, Pagination? pagination);

    Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, Pagination? pagination);

    Task<DelegationEligibilityResponse> CheckEligibilityAsync(int contactId, int accountId);

    Task<ProcessDelegationRequestsResponse> AcceptRequestsAsync(int currentUserId, AcceptDelegationRequestsRequest request);

    Task<ProcessDelegationRequestsResponse> RefuseRequestsAsync(int currentUserId, RefuseDelegationRequestsRequest request);
}
