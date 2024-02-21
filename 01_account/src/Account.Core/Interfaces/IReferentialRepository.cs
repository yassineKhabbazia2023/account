// <copyright file="IReferentialRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IReferentialRepository
    {
        Task<IEnumerable<Hub?>> GetHubsAsync();

        Task<IEnumerable<Naf?>> GetNafsAsync();
    }
}
