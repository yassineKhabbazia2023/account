// <copyright file="OfferEligibilityRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Providers;

public class OfferEligibilityRepository : IOfferEligibilityRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public OfferEligibilityRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<OfferEligibility?> GetOfferEligibilityByIdAsync(int accountId)
    {
        var offerEligibilityEntity = await _accountContext.OfferEligibilityEntity.FirstOrDefaultAsync(o => o.AccountId == accountId);
        return offerEligibilityEntity.MapToOfferEligibility();
    }

    public async Task<bool> IsOfferEligibilityActiveAsync(int accountId)
    {
        return await _accountContext.OfferEligibilityEntity
                                    .AnyAsync(o => o.AccountId == accountId
                                                && o.ApprovedDate.HasValue
                                                && o.ApprovedDate != DateTime.MinValue);
    }

    public async Task<OfferEligibility> UpdateOfferEligibilityAsync(int accountId, string approvedBy)
    {
        OfferEligibility? toReturn = null!;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var existingOfferEligibility = await _accountContext.OfferEligibilityEntity.FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (existingOfferEligibility == null)
            {
                throw new NotFoundException(Errors.NotFoundOfferEligibilityCode, string.Format(Errors.NotFoundOfferEligibilityMessage, accountId));
            }

            existingOfferEligibility.MapToActivatedOfferEligibility(approvedBy);
            _accountContext.OfferEligibilityEntity.Update(existingOfferEligibility);
            await _accountContext.SaveChangesAsync();
            toReturn = existingOfferEligibility.MapToOfferEligibility();
        });

        return toReturn!;
    }
}
