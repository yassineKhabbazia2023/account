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
    private readonly IRoleRepository _roleRepository;
    private readonly IOfferActivatedEventPublisher _offerActivatedEventPublisher;
    private readonly IReportEventPublisher _reportEventPublisher;

    public OfferEligibilityService(IOfferEligibilityRepository offerEligibilityRepository, IContactRepository contactRepository, IRoleRepository roleRepository, IOfferActivatedEventPublisher offerActivatedEventPublisher, IReportEventPublisher reportEventPublisher)
    {
        _offerEligibilityRepository = offerEligibilityRepository;
        _contactRepository = contactRepository;
        _roleRepository = roleRepository;
        _offerActivatedEventPublisher = offerActivatedEventPublisher;
        _reportEventPublisher = reportEventPublisher;
    }

    public async Task<OfferEligibility?> GetOfferEligibilityByIdAsync(int accountId)
    {
        return await _offerEligibilityRepository.GetOfferEligibilityByIdAsync(accountId);
    }

    public async Task<OfferEligibility> UpdateOfferEligibilityAsync(int currentUserId, int accountId)
    {
        var contact = await _contactRepository.GetContactByIdAsync(currentUserId);

        if (ContactType.Collaborator.ToString().Equals(contact.Type))
        {
            throw new BadRequestException(Errors.NotPermittedActionCode, Errors.NotPermittedActionMessage);
        }

        if (!await _roleRepository.IsContactHasRoleOnAccount(currentUserId, accountId, null))
        {
            throw new ForbiddenException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, currentUserId, accountId));
        }

        var isAlreadyActive = await _offerEligibilityRepository.IsOfferEligibilityActiveAsync(accountId);
        if (isAlreadyActive)
        {
            throw new BadRequestException(Errors.AlreadyActiveOfferEligibilityCode, Errors.AlreadyActiveOfferEligibilityMessage);
        }

        var offerEligibility = await _offerEligibilityRepository.UpdateOfferEligibilityAsync(accountId, contact.Email);

        await _offerActivatedEventPublisher.PublishOfferActivatedEventAsync(offerEligibility.AccountId, offerEligibility.OfferName);

        if (offerEligibility.Reporting != null)
        {
            var reporting = offerEligibility.Reporting;
            await _reportEventPublisher.PublishReportCreatedEventAsync(reporting.ReportId, accountId, GlobalConstants.CLARITYREPORTTYPEID, reporting.ReportLabel, ReportStatus.ONLINE);
        }

        return offerEligibility;
    }
}
