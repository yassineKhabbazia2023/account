// <copyright file="Account.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Models;

public class Account
{
    public int AccountId { get; set; }

    public required string AccountNumber { get; set; }

    public required string LegalName { get; set; }

    public bool IsFavorite { get; set; }

    public required Address Address { get; set; }

    public Owner? Owner { get; set; }

    public Deployment? Deployment { get; set; }
}
