// <copyright file="DelegationRequestTestHelper.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Tests.Helpers;

internal static class DelegationRequestTestHelper
{
    /// <summary>
    /// Simulates ExecuteUpdateAsync for testing purposes (updates requests by IDs).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task SimulateUpdateRequestsByIdsAsync(AccountContext context, int[] delegationRequestIds, string status, DateTime respondedAt)
    {
        var requests = context.DelegationRequestEntity
            .Where(dr => delegationRequestIds.Contains(dr.DelegationRequestId))
            .ToList();

        foreach (var request in requests)
        {
            request.Status = status;
            request.RespondedAt = respondedAt;
        }

        context.DelegationRequestEntity.UpdateRange(requests);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Simulates ExecuteUpdateAsync for testing purposes (updates sibling requests by requester/account).
    /// </summary>
    internal static async Task SimulateUpdateSiblingRequestsAsync(AccountContext context, int requesterId, int accountId, string status, DateTime respondedAt, string? onlyWithStatus = null)
    {
        var query = context.DelegationRequestEntity
            .Where(dr => dr.RequesterId == requesterId && dr.AccountId == accountId);

        if (onlyWithStatus != null)
        {
            query = query.Where(dr => dr.Status == onlyWithStatus);
        }

        var requests = query.ToList();

        foreach (var request in requests)
        {
            request.Status = status;
            request.RespondedAt = respondedAt;
        }

        context.DelegationRequestEntity.UpdateRange(requests);
        await context.SaveChangesAsync();
    }
}
