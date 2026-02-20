// <copyright file="VentyaService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services;

public class VentyaService : IVentyaService
{
    private readonly IVentyaRepository _ventyaRepository;
    private readonly ILogger<VentyaService> _logger;

    public VentyaService(IVentyaRepository ventyaRepository, ILogger<VentyaService> logger)
    {
        _ventyaRepository = ventyaRepository;
        _logger = logger;
    }

    public async Task<bool> CheckVentyaAccessAsync(int accountId, int contactId)
    {
        if (accountId <= 0)
        {
            _logger.LogWarning("Invalid accountId '{AccountId}' provided", accountId);
            return false;
        }

        var result = await _ventyaRepository.CheckVentyaAccessAsync(accountId, contactId);

        if (!result.AccountFound)
        {
            _logger.LogWarning("Account with id '{AccountId}' not found", accountId);
        }
        else if (!result.ContactFound)
        {
            _logger.LogWarning("Contact with id '{ContactId}' not found", contactId);
        }
        else if (!result.RoleFound)
        {
            _logger.LogWarning("Role for contact '{ContactId}' on account id '{AccountId}' not found", contactId, accountId);
        }

        return result.HasAccess;
    }

    public async Task<string?> GetVentyaAccessContactEmailAsync(int accountId)
    {
        if (accountId <= 0)
        {
            return null;
        }

        return await _ventyaRepository.GetVentyaAccessContactEmailAsync(accountId);
    }

    public async Task<Result<DematReadyResponse>> CheckAccountIsDematReadyAsync(int accountId)
    {
        if (accountId <= 0)
        {
            return Result<DematReadyResponse>.NotFound();
        }

        var accountEmailResult = await _ventyaRepository.GetAccountEmailAsync(accountId);
        if (!accountEmailResult.AccountExists)
        {
            return Result<DematReadyResponse>.NotFound();
        }

        var contactId = await _ventyaRepository.GetSsoContactIdAsync(accountId);
        var contactWithAccessEmail = await _ventyaRepository.GetVentyaAccessContactEmailAsync(accountId);
        var hasAccountEmail = !string.IsNullOrWhiteSpace(accountEmailResult.AccountEmail);
        var isReady = hasAccountEmail && contactId.HasValue;

        return Result<DematReadyResponse>.Success(new DematReadyResponse
        {
            IsReady = isReady,
            ContactWithAccess = contactWithAccessEmail,
            ExternalDematMail = accountEmailResult.AccountEmail,
        });
    }
}
