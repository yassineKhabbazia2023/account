// <copyright file="OfferEligibilityEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Providers;

public class OfferEligibilityEventRepository : IOfferEligibilityEventRepository
{
    private readonly AccountContext _accountContext;

    public OfferEligibilityEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();
    }

    public async Task CreateOfferEligibilityAsync(OfferEligibilityEntity offerEligibility)
    {
        await _accountContext.OfferEligibilityEntity.AddAsync(offerEligibility);
        await _accountContext.SaveChangesAsync();
    }

    public async Task<bool> DoesOfferEligibilityExistsAsync(int accountId)
    {
        return await _accountContext.OfferEligibilityEntity.AnyAsync(o => o.AccountId == accountId);
    }
}
