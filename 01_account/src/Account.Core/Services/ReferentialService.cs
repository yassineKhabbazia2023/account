// <copyright file="ReferentialService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Services
{
    public class ReferentialService : IReferentialService
    {
        private readonly IReferentialRepository _referentialRepository;

        public ReferentialService(IReferentialRepository referentialRepository)
        {
            _referentialRepository = referentialRepository;
        }

        public async Task<IEnumerable<Hub?>> GetHubsAsync()
        {
            return await _referentialRepository.GetHubsAsync();
        }

        public async Task<IEnumerable<Naf?>> GetNafsAsync()
        {
            return await _referentialRepository.GetNafsAsync();
        }
    }
}
