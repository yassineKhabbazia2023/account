// <copyright file="ISerenityService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface ISerenityService
{
    Task<SerenityEligibility> GetSerenityEligibilityAsync(int contactId);

    Task CreateSerenityChoiceAsync(int contactId, bool isAccepted);
}
