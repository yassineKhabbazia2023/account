// <copyright file="CreateDelegationRequestsRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Requests;

public class CreateDelegationRequestsRequest
{
    [Required(ErrorMessage = "AccountId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AccountId must be greater than 0")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "RecipientIds is required")]
    [MinLength(1, ErrorMessage = "At least one recipient is required")]
    public int[] RecipientIds { get; set; } = Array.Empty<int>();
}