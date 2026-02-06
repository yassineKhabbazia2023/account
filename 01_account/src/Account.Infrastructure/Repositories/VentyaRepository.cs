// <copyright file="VentyaRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
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

    public async Task<(bool AccountExists, string? AccountEmail)> GetAccountEmailAsync(string accountNumber)
    {
        (bool AccountExists, string? AccountEmail) result = (false, null);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var account = await _accountContext.AccountEntity
                .AsNoTracking()
                .Where(account => account.AccountNumber == accountNumber)
                .Select(account => new { account.Email })
                .FirstOrDefaultAsync();

            result = account == null
                ? (false, null)
                : (true, account.Email);
        });

        return result;
    }

    public async Task<int?> GetSsoContactIdAsync(string accountNumber)
    {
        int? result = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactIds = await _accountContext.RoleEntity
                .AsNoTracking()
                .Join(
                    _accountContext.AccountEntity.AsNoTracking().Where(account => account.AccountNumber == accountNumber),
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
