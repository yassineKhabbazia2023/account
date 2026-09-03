// <copyright file="IDelegationRequestEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRequestEventPublisher
{
    Task PublishDelegationRequestCreatedEventAsync(AccountDetail account, IEnumerable<int> recipientIds);

    Task PublishDelegationRequestValidatedEventAsync(int validatorContactId, List<DelegationRequest> delegationRequests);

    Task PublishDelegationRequestRefusedEventAsync(int refuserContactId, int requesterContactId, int accountId, string? accountType);
}
