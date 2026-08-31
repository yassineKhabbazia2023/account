// <copyright file="SerenityService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Services;

public class SerenityService : ISerenityService
{
    private readonly ISerenityRepository _serenityRepository;

    public SerenityService(ISerenityRepository serenityRepository)
    {
        _serenityRepository = serenityRepository;
    }

    public async Task<SerenityEligibility> GetSerenityEligibilityAsync(int contactId)
    {
        return await _serenityRepository.GetSerenityEligibilityAsync(contactId);
    }

    public async Task CreateSerenityChoiceAsync(int contactId, bool isAccepted)
    {
        await _serenityRepository.CreateSerenityChoiceAsync(contactId, isAccepted);
    }
}
