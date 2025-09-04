// <copyright file="IOfferEligibilityRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IOfferEligibilityRepository
{
    Task<OfferEligibility?> GetOfferEligibilityByIdAsync(int accountId);

    Task<bool> IsOfferEligibilityActiveAsync(int accountId);

    Task<OfferEligibility> UpdateOfferEligibilityAsync(int accountId, string approvedBy);
}
