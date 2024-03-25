// <copyright file="IReferentialService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IReferentialService
    {
        Task<IEnumerable<Hub?>> GetHubsAsync();

        Task<Paging<Naf>> GetNafsAsync(string? search, int pageNumber, int pageSize);

        AccountReferentialInformation GetAccountReferentialInformation();
    }
}
