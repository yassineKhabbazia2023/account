// <copyright file="CreateDelegation.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Dtos;

public class CreateDelegation
{
    [JsonRequired]
    public Guid GlobalAccountId { get; set; }

    [JsonRequired]
    public Guid GlobalDelegatorId { get; set; }

    [JsonRequired]
    public Guid GlobalDelegateeId { get; set; }

    [JsonRequired]
    public DateTime StartDate { get; set; }

    [JsonRequired]
    public DateTime EndDate { get; set; }

    public string? Note { get; set; }
}
