// <copyright file="VentyaRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Context;

namespace Pulse.Account.Infrastructure.Repositories;

public class VentyaRepository : IVentyaRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public VentyaRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;

        _retryPolicy = Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<VentyaAccessResult> CheckVentyaAccessAsync(int accountId, int contactId)
    {
        var result = new VentyaAccessResult();

        var account = await _accountContext.AccountEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountId == accountId);

        if (account == null)
        {
            return result;
        }

        result.AccountFound = true;

        var contact = await _accountContext.ContactEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContactId == contactId);

        if (contact == null)
        {
            return result;
        }

        result.ContactFound = true;

        if (contact.Type != ContactType.Customer.ToString())
        {
            return result;
        }

        var role = await _accountContext.RoleEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contactId);

        if (role == null)
        {
            return result;
        }

        result.RoleFound = true;
        result.HasAccess = role.ContactFlagPortailFactures.HasValue && role.ContactFlagPortailFactures.Value == true;

        return result;
    }

    public async Task<(bool AccountExists, string? AccountEmail)> GetAccountEmailAsync(int accountId)
    {
        (bool AccountExists, string? AccountEmail) result = (false, null);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var account = await _accountContext.AccountEntity
                .AsNoTracking()
                .Where(account => account.AccountId == accountId)
                .Select(account => new { account.Email })
                .FirstOrDefaultAsync();

            result = account == null
                ? (false, null)
                : (true, account.Email);
        });

        return result;
    }

    public async Task<string?> GetVentyaAccessContactEmailAsync(int accountId)
    {
        string? result = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var emails = await _accountContext.RoleEntity
                .AsNoTracking()
                .Join(
                    _accountContext.AccountEntity.AsNoTracking().Where(account => account.AccountId == accountId),
                    role => role.AccountId,
                    account => account.AccountId,
                    (role, account) => role)
                .Join(
                    _accountContext.ContactEntity.AsNoTracking(),
                    role => role.ContactId,
                    contact => contact.ContactId,
                    (role, contact) => new { role.ContactFlagPortailFactures, contact.Email })
                .Where(role => role.ContactFlagPortailFactures == true)
                .Select(role => role.Email)
                .Take(2)
                .ToListAsync();

            result = emails.Count == 1 ? emails[0] : null;
        });

        return result;
    }

    public async Task<int?> GetSsoContactIdAsync(int accountId)
    {
        int? result = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactIds = await _accountContext.RoleEntity
                .AsNoTracking()
                .Join(
                    _accountContext.AccountEntity.AsNoTracking().Where(account => account.AccountId == accountId),
                    role => role.AccountId,
                    account => account.AccountId,
                    (role, account) => role)
                .Where(role => role.ContactFlagPortailFactures == true)
                .Select(role => role.ContactId)
                .Take(2)
                .ToListAsync();

            result = contactIds.Count == 1 ? contactIds[0] : null;
        });

        return result;
    }
}
