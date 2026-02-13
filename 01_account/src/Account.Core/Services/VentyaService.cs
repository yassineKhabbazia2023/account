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

    public async Task<bool> CheckVentyaAccessAsync(string accountNumber, int contactId)
    {
        var result = await _ventyaRepository.CheckVentyaAccessAsync(accountNumber, contactId);

        if (!result.AccountFound)
        {
            _logger.LogWarning("Account with number '{AccountNumber}' not found", accountNumber);
        }
        else if (!result.ContactFound)
        {
            _logger.LogWarning("Contact with id '{ContactId}' not found", contactId);
        }
        else if (!result.RoleFound)
        {
            _logger.LogWarning("Role for contact '{ContactId}' on account '{AccountNumber}' not found", contactId, accountNumber);
        }

        return result.HasAccess;
    }

    public async Task<string?> GetVentyaAccessContactEmailAsync(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return null;
        }

        return await _ventyaRepository.GetVentyaAccessContactEmailAsync(accountNumber);
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
        var contactWithAccessEmail = await _ventyaRepository.GetVentyaAccessContactEmailAsync(accountNumber);
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
