// <copyright file="CreateDelegationRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Requests;

public class CreateDelegationRequest
{
    required public int DelegatorId { get; set; }

    required public IEnumerable<DelegationDetails> DelegationDetails { get; set; }

    required public IEnumerable<int> AccountIds { get; set; }
}
