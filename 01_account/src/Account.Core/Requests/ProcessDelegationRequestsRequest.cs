// <copyright file="ProcessDelegationRequestsRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests;

public abstract class ProcessDelegationRequestsRequest
{
    public int[] DelegationRequestIds { get; set; } = Array.Empty<int>();
}

