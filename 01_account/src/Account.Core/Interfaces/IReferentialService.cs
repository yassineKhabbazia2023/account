// <copyright file="IReferentialService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces
{
    public interface IReferentialService
    {
        Task<IEnumerable<Hub?>> GetHubsAsync();

        Task<Paging<Naf>> GetNafsAsync(string? search, Pagination? pagination);

        AccountReferentialInformation GetAccountReferentialInformation();
    }
}
