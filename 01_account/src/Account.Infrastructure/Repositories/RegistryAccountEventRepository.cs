// <copyright file="RegistryAccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class RegistryAccountEventRepository : IRegistryAccountEventRepository
{
    private readonly AccountContext _context;
    private readonly ILogger<RegistryAccountEventRepository> _logger;

    public RegistryAccountEventRepository(AccountContext context, ILogger<RegistryAccountEventRepository> logger)
    {
        _context = context;
        _logger = logger;
        _context.HandleEFCoreFailure();
    }

    public async Task<AccountDetail> CreateAccountAsync(RegistryAccountCreatedEventData eventData)
    {
        var naf = await GetNafByCodeAsync(eventData.AccountNafIdentifier);
        eventData.AccountNafIdentifier = naf?.NafId.ToString();

        var accountEntity = eventData.ToAccountEntity();

        _context.AccountEntity.Add(accountEntity);
        await _context.SaveChangesAsync();
        return accountEntity.MapToAccountDetail()!;
    }

    public async Task<int> RemoveAccountAsync(Guid accountGlobalUniqueIdentifier)
    {
        var accountToRemove = _context.AccountEntity.FirstOrDefault(a => a.AccountGlobalUniqueId == accountGlobalUniqueIdentifier);

        if (accountToRemove == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountGlobalUniqueIdentifier));
        }

        accountToRemove.IsActive = false;

        _context.AccountEntity.Update(accountToRemove);
        await _context.SaveChangesAsync();
        return accountToRemove.AccountId;
    }

    public async Task<AccountDetail> UpdateAccountAsync(RegistryAccountUpdatedEventData eventData)
    {
        var naf = await GetNafByCodeAsync(eventData.AccountNafIdentifier);
        eventData.AccountNafIdentifier = naf?.NafId.ToString();

        var existingAccount = await _context.AccountEntity
            .Include(a => a.DeploymentEntity)
            .Include(a => a.AddressEntity)
            .Include(a => a.PhoneEntity)
            .FirstOrDefaultAsync(a => a.AccountGlobalUniqueId == eventData.AccountGlobalUniqueIdentifier);

        if (existingAccount == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, eventData.AccountGlobalUniqueIdentifier));
        }

        var newAccount = eventData.ToAccountEntity(false);

        // Protéger les champs obligatoires : ne pas permettre de les vider une fois renseignés
        ProtectRequiredFields(existingAccount, newAccount);

        existingAccount.ToAccountEntity(newAccount, false);
        await _context.SaveChangesAsync();

        return existingAccount.MapToAccountDetail()!;
    }

    private void ProtectRequiredFields(AccountEntity existingAccount, AccountEntity newAccount)
    {
        // Protéger StaffSizeRange
        if (!string.IsNullOrWhiteSpace(existingAccount.StaffSizeRange)
            && string.IsNullOrWhiteSpace(newAccount.StaffSizeRange))
        {
            _logger.LogError(
                "Tentative de suppression du champ StaffSizeRange pour le compte {AccountId} via événement Registry. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.",
                existingAccount.AccountId,
                existingAccount.StaffSizeRange);

            // Conserver la valeur existante
            newAccount.StaffSizeRange = existingAccount.StaffSizeRange;
        }

        // Protéger AccountingMethod
        if (!string.IsNullOrWhiteSpace(existingAccount.AccountingMethod)
            && string.IsNullOrWhiteSpace(newAccount.AccountingMethod))
        {
            _logger.LogError(
                "Tentative de suppression du champ AccountingMethod pour le compte {AccountId} via événement Registry. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.",
                existingAccount.AccountId,
                existingAccount.AccountingMethod);

            // Conserver la valeur existante
            newAccount.AccountingMethod = existingAccount.AccountingMethod;
        }
    }

    public async Task<bool> DoesAccountExistAsync(Guid accountGlobalUniqueId)
    {
        return await _context.AccountEntity.AnyAsync(a => a.AccountGlobalUniqueId == accountGlobalUniqueId);
    }

    private async Task<Naf?> GetNafByCodeAsync(string? nafCode)
    {
        NafEntity? naf = await _context.NafEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.NafCode.Replace(".", string.Empty) == nafCode);

        return naf?.MapToNaf();
    }
}
