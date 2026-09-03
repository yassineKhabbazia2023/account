// <copyright file="EmailOptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Options;

public class EmailOptions
{
    public const string Section = "EmailSettings";

    [Required]
    public required string DelegationRequestEmailTemplate { get; set; }

    [Required]
    public required string DelegationRequestRefusedEmailTemplate { get; set; }

    [Required]
    public required string SenderEmail { get; set; }

    [Required]
    public required string ServiceBusTopic { get; set; }

    [Required]
    public required string WalletBaseUrl { get; set; }
}
