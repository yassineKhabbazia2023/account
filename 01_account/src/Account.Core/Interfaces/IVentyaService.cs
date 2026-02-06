// <copyright file="IVentyaService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces;

public interface IVentyaService
{
    Task<Result<DematReadyResponse>> CheckAccountIsDematReadyAsync(string accountNumber);
}
