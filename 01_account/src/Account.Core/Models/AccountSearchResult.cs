// <copyright file="AccountSearchResult.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class AccountSearchResult
{
    public int AccountId { get; set; }

    public string? LegalName { get; set; }

    public string? AccountNumber { get; set; }

    public string? Siret { get; set; }

    public string? DirectorEmail { get; set; }
}
