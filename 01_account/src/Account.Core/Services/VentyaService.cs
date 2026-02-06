// <copyright file="VentyaService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services;

public class VentyaService : IVentyaService
{
    private readonly IVentyaRepository _ventyaRepository;

    public VentyaService(IVentyaRepository ventyaRepository)
    {
        _ventyaRepository = ventyaRepository;
    }

    public async Task<Result<DematReadyResponse>> CheckAccountIsDematReadyAsync(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return Result<DematReadyResponse>.NotFound();
        }

        var accountEmailResult = await _ventyaRepository.GetAccountEmailAsync(accountNumber);
        if (!accountEmailResult.AccountExists)
        {
            return Result<DematReadyResponse>.NotFound();
        }

        var contactId = await _ventyaRepository.GetSsoContactIdAsync(accountNumber);
        var hasAccountEmail = !string.IsNullOrWhiteSpace(accountEmailResult.AccountEmail);
        var isReady = hasAccountEmail && contactId.HasValue;

        return Result<DematReadyResponse>.Success(new DematReadyResponse { IsReady = isReady });
    }
}
