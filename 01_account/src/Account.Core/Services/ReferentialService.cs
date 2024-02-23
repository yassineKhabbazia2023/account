// <copyright file="ReferentialService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

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

        public async Task<Paging<Naf>> GetNafsAsync(string? search, int pageNumber, int pageSize)
        {
            pageNumber = Pagination.GetValidPageNumber(pageNumber);
            pageSize = Pagination.GetValidPageSize(pageSize);

            return await _referentialRepository.GetNafsAsync(search, pageNumber, pageSize);
        }
    }
}
