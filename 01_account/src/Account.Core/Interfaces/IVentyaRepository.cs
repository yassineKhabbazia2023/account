// <copyright file="IVentyaRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IVentyaRepository
{
    Task<VentyaAccessResult> CheckVentyaAccessAsync(string accountNumber, int contactId);

    Task<(bool AccountExists, string? AccountEmail)> GetAccountEmailAsync(string accountNumber);

    Task<string?> GetVentyaAccessContactEmailAsync(string accountNumber);

    Task<int?> GetSsoContactIdAsync(string accountNumber);
}

public class VentyaAccessResult
{
    public bool HasAccess { get; set; }

    public bool AccountFound { get; set; }

    public bool ContactFound { get; set; }

    public bool RoleFound { get; set; }
}
