// <copyright file="IVentyaRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IVentyaRepository
{
    Task<(bool AccountExists, string? AccountEmail)> GetAccountEmailAsync(string accountNumber);

    Task<int?> GetSsoContactIdAsync(string accountNumber);
}
