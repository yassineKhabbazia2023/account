// <copyright file="OfferEligibilityService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class OfferEligibilityService : IOfferEligibilityService
{
    private readonly IOfferEligibilityRepository _offerEligibilityRepository;
    private readonly IContactRepository _contactRepository;

    public OfferEligibilityService(IOfferEligibilityRepository offerEligibilityRepository, IContactRepository contactRepository)
    {
        _offerEligibilityRepository = offerEligibilityRepository;
        _contactRepository = contactRepository;
    }

    public async Task<OfferEligibility?> GetOfferEligibilityByIdAsync(int accountId)
    {
        return await _offerEligibilityRepository.GetOfferEligibilityByIdAsync(accountId);
    }

    public async Task<OfferEligibility> UpdateOfferEligibilityAsync(int currentUserId, int accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(currentUserId);

        if (contact.Type!.Equals(ContactType.Collaborator.ToString()))
        {
            throw new BadRequestException(Errors.NotPermittedActionCode, Errors.NotPermittedActionMessage);
        }

        var isAlreadyActive = await _offerEligibilityRepository.IsOfferEligibilityActiveAsync(accountId);
        if (isAlreadyActive)
        {
            throw new BadRequestException(Errors.AlreadyActiveOfferEligibilityCode, Errors.AlreadyActiveOfferEligibilityMessage);
        }

        return await _offerEligibilityRepository.UpdateOfferEligibilityAsync(accountId, contact.Email);
    }
}
