// <copyright file="DelegationRequestService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class DelegationRequestService : IDelegationRequestService
{
    private readonly IDelegationRequestRepository _delegationRequestRepository;

    public DelegationRequestService(IDelegationRequestRepository delegationRequestRepository)
    {
        _delegationRequestRepository = delegationRequestRepository;
    }

    public async Task CreateDelegationRequestsAsync(int contactId, CreateDelegationRequestsRequest request)
    {
        // Validate payload (empty or duplicates)
        if (request.RecipientIds == null || request.RecipientIds.Length == 0)
        {
            throw new BadRequestException(Errors.CreateDelegationCode, Errors.CreateDelegationMessage);
        }

        var distinctRecipientIds = request.RecipientIds.Distinct().ToArray();
        if (distinctRecipientIds.Length != request.RecipientIds.Length)
        {
            throw new BadRequestException(Errors.CreateDelegationCode, Errors.CreateDelegationMessage);
        }

        // Validate self-delegation
        if (request.RecipientIds.Contains(contactId))
        {
            throw new BadRequestException(Errors.SelfDelegationRequestCode, Errors.SelfDelegationRequestMessage);
        }

        // Verify account exists
        if (!await _delegationRequestRepository.DoesAccountExistAsync(request.AccountId))
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, request.AccountId));
        }

        // Verify requester exists
        if (!await _delegationRequestRepository.DoesContactExistAsync(contactId))
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        // Verify all recipients exist
        foreach (var recipientId in request.RecipientIds)
        {
            if (!await _delegationRequestRepository.DoesContactExistAsync(recipientId))
            {
                throw new NotFoundException(Errors.NotFoundContactsCode, Errors.NotFoundContactsMessage);
            }
        }

        // Verify requester does not already have access
        if (await _delegationRequestRepository.HasRequesterAccessToAccountAsync(contactId, request.AccountId))
        {
            throw new BadRequestException(Errors.RequesterAlreadyHasAccessCode, Errors.RequesterAlreadyHasAccessMessage);
        }

        // Verify all recipients have access to the account
        foreach (var recipientId in request.RecipientIds)
        {
            if (!await _delegationRequestRepository.HasRecipientAccessToAccountAsync(recipientId, request.AccountId))
            {
                throw new BadRequestException(Errors.RecipientDoesNotHaveAccessCode, string.Format(Errors.RecipientDoesNotHaveAccessMessage, recipientId));
            }
        }

        // Verify no pending request exists
        if (await _delegationRequestRepository.HasPendingRequestAsync(contactId, request.AccountId))
        {
            throw new BadRequestException(Errors.DelegationRequestAlreadyPendingCode, Errors.DelegationRequestAlreadyPendingMessage);
        }

        // Create delegation requests with pending status
        var pendingStatus = DelegationRequestStatus.Pending.ToString().ToLower();
        await _delegationRequestRepository.CreateDelegationRequestsAsync(contactId, request.AccountId, request.RecipientIds, pendingStatus);
    }

    public async Task<Paging<DelegationRequest>> GetSentRequestsAsync(int contactId, Pagination? pagination)
    {
        pagination ??= new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        return await _delegationRequestRepository.GetSentRequestsAsync(contactId, null, pagination);
    }

    public async Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, Pagination? pagination)
    {
        pagination ??= new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        // Only return pending requests (main use case)
        var pendingStatus = DelegationRequestStatus.Pending.ToString().ToLower();
        return await _delegationRequestRepository.GetReceivedRequestsAsync(contactId, pendingStatus, pagination);
    }

    public async Task<DelegationEligibilityResponse> CheckEligibilityAsync(int contactId, int accountId)
    {
        // Check if user already has access to the account (via existing role)
        if (await _delegationRequestRepository.HasRequesterAccessToAccountAsync(contactId, accountId))
        {
            return new DelegationEligibilityResponse
            {
                IsEligible = false,
                Reason = DelegationEligibilityReason.AlreadyInPortfolio
            };
        }

        // Check if pending request exists
        if (await _delegationRequestRepository.HasPendingRequestAsync(contactId, accountId))
        {
            return new DelegationEligibilityResponse
            {
                IsEligible = false,
                Reason = DelegationEligibilityReason.AlreadyPending
            };
        }

        // User is eligible
        return new DelegationEligibilityResponse
        {
            IsEligible = true,
            Reason = null
        };
    }
}