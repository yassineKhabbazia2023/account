// <copyright file="IOfferEligibilityEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IOfferEligibilityEventRepository
{
    Task CreateOfferEligibilityAsync(OfferEligibilityEntity offerEligibility);

    Task<bool> DoesOfferEligibilityExistsAsync(int accountId);
}
