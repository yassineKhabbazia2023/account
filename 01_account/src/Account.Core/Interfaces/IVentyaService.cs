// <copyright file="IVentyaService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces;

public interface IVentyaService
{
    Task<bool> CheckVentyaAccessAsync(int accountId, int contactId);

    Task<string?> GetVentyaAccessContactEmailAsync(int accountId);

    Task<Result<DematReadyResponse>> CheckAccountIsDematReadyAsync(int accountId);
}
