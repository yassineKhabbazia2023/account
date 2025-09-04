// <copyright file="IOfferEligibilityService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.JsonPatch;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IOfferEligibilityService
{
    Task<OfferEligibility?> GetOfferEligibilityByIdAsync(int accountId);

    Task<OfferEligibility> UpdateOfferEligibilityAsync(int currentUserId, int accountId);
}
