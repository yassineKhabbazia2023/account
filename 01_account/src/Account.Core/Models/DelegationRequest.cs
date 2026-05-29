// <copyright file="DelegationRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Models;

public class DelegationRequest
{
    [JsonIgnore]
    public int DelegationRequestId { get; set; }

    public int RequesterId { get; set; }

    public int RecipientId { get; set; }

    public int AccountId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Status { get; set; }

    public DateTime? RespondedAt { get; set; }

    public Contact? Requester { get; set; }

    public Contact? Recipient { get; set; }

    public Account? Account { get; set; }
}