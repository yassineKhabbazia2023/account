// <copyright file="IReferentialService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IReferentialService
    {
        Task<IEnumerable<Hub?>> GetHubsAsync();
    }
}
