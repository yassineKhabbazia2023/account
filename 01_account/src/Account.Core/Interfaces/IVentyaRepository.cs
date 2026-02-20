// <copyright file="IVentyaRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IVentyaRepository
{
    Task<VentyaAccessResult> CheckVentyaAccessAsync(int accountId, int contactId);

    Task<(bool AccountExists, string? AccountEmail)> GetAccountEmailAsync(int accountId);

    Task<string?> GetVentyaAccessContactEmailAsync(int accountId);

    Task<int?> GetSsoContactIdAsync(int accountId);
}

public class VentyaAccessResult
{
    public bool HasAccess { get; set; }

    public bool AccountFound { get; set; }

    public bool ContactFound { get; set; }

    public bool RoleFound { get; set; }
}
