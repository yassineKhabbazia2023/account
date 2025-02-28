// <copyright file="RegistryAccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
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

    public RegistryAccountEventRepository(AccountContext context)
    {
        _context = context;
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

        var deploymentEntity = accountToRemove.DeploymentEntity?.FirstOrDefault();

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

        var newAccount = eventData.ToAccountEntity();
        existingAccount.ToAccountEntity(newAccount);
        await _context.SaveChangesAsync();

        return existingAccount.MapToAccountDetail()!;
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
