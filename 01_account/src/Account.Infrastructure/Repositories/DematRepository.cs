// <copyright file="DematRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Repositories;

public class DematRepository : IDematRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public DematRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;

        _retryPolicy = Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<DateTime?> GetDematModalClosedDateAsync(int accountId, int contactId)
    {
        DateTime? result = null;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            result = await _accountContext.DematModalClosureEntity
                .AsNoTracking()
                .Where(closure => closure.AccountId == accountId && closure.ContactId == contactId)
                .Select(closure => (DateTime?)closure.ClosedDate)
                .FirstOrDefaultAsync();
        });

        return result;
    }

    public async Task CloseDematModalAsync(int accountId, int contactId)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var alreadyClosed = await _accountContext.DematModalClosureEntity
                .AsNoTracking()
                .AnyAsync(closure => closure.AccountId == accountId && closure.ContactId == contactId);

            if (alreadyClosed)
            {
                return;
            }

            _accountContext.DematModalClosureEntity.Add(new DematModalClosureEntity
            {
                AccountId = accountId,
                ContactId = contactId,
                ClosedDate = DateTime.UtcNow,
            });

            await _accountContext.SaveChangesAsync();
        });
    }
}
