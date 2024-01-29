// <copyright file="Account.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Models;

public class Account
{
    public int AccountId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string LegalName { get; set; } = null!;

    public bool IsFavorite { get; set; }

    public Address Address { get; set; } = null!;

    public Owner? Owner { get; set; }

    public Deployment? Deployment { get; set; }
}
