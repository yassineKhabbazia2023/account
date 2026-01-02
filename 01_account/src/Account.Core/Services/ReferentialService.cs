// <copyright file="ReferentialService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services;

public class ReferentialService : IReferentialService
{
    private readonly IReferentialRepository _referentialRepository;

    public ReferentialService(IReferentialRepository referentialRepository)
    {
        _referentialRepository = referentialRepository;
    }

    public async Task<IEnumerable<Hub?>> GetHubsAsync(string? sort = null)
    {
        var result = await _referentialRepository.GetHubsAsync();

        // Sort parameter to order hubs by name (ASC/DESC).
        if (string.Equals(sort, "ASC", StringComparison.OrdinalIgnoreCase))
        {
            result = result.OrderBy(hub => hub?.HubName).ToList();
        }
        else if (string.Equals(sort, "DESC", StringComparison.OrdinalIgnoreCase))
        {
            result = result.OrderByDescending(hub => hub?.HubName).ToList();
        }

        return result;
    }

    public async Task<Paging<Naf>> GetNafsAsync(string? search, Pagination? pagination)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        return await _referentialRepository.GetNafsAsync(search, pagination);
    }

    public AccountReferentialInformation GetAccountReferentialInformation()
    {
        return new AccountReferentialInformation();
    }

    public async Task<IEnumerable<Office?>> GetOfficesAsync()
        => await _referentialRepository.GetOfficesAsync();
}
