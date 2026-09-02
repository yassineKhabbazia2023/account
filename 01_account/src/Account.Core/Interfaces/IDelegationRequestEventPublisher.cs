// <copyright file="IDelegationRequestEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IDelegationRequestEventPublisher
{
    Task PublishDelegationRequestValidatedEventAsync(int validatorContactId, List<DelegationRequest> delegationRequests);
}
