// <copyright file="CreateDelegation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Requests;

public class CreateDelegation
{
    [JsonRequired]
    public int AccountId { get; set; }

    [JsonRequired]
    public int DelegatorId { get; set; }

    [JsonRequired]
    public int DelegateeId { get; set; }

    [JsonRequired]
    public DateTime StartDate { get; set; }

    [JsonRequired]
    public DateTime EndDate { get; set; }

    public string? Note { get; set; }
}
