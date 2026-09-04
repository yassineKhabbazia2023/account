// <copyright file="DelegationRequestService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class DelegationRequestService(IDelegationRequestRepository delegationRequestRepository,
    IDelegationService delegationService,
    IEmailService emailService,
    IContactRepository contactRepository,
    IAccountRepository accountRepository,
    IDelegationRequestEventPublisher delegationRequestEventPublisher,
    ILogger<DelegationRequestService> logger) : IDelegationRequestService
{
    private static readonly string[] DefaultStatuses = { DelegationStatusValues.Pending };

    private readonly IDelegationRequestRepository _delegationRequestRepository = delegationRequestRepository;
    private readonly IDelegationService _delegationService = delegationService;
    private readonly IEmailService _emailService = emailService;
    private readonly IContactRepository _contactRepository = contactRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IDelegationRequestEventPublisher _delegationRequestEventPublisher = delegationRequestEventPublisher;
    private readonly ILogger<DelegationRequestService> _logger = logger;

    public async Task<CreateDelegationRequestsResponse> CreateDelegationRequestsAsync(int contactId, CreateDelegationRequestsRequest request)
    {
        var requestor = await _contactRepository.GetContactByIdAsync(contactId);

        if (!ContactType.Collaborator.ToString().Equals(requestor.Type, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new UnauthorizedException(Errors.BadRequestClientCannotRequestDelegationCode, Errors.BadRequestClientCannotRequestDelegationMessage);
        }

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

        // Verify requester does not already have access (via role or active delegation)
        if (await HasAccessToAccountAsync(contactId, request.AccountId))
        {
            throw new BadRequestException(Errors.RequesterAlreadyHasAccessCode, Errors.RequesterAlreadyHasAccessMessage);
        }

        // Verify no pending request exists
        if (await _delegationRequestRepository.HasPendingRequestAsync(contactId, request.AccountId))
        {
            throw new BadRequestException(Errors.DelegationRequestAlreadyPendingCode, Errors.DelegationRequestAlreadyPendingMessage);
        }

        // Validate each recipient individually (partial success approach)
        var validRecipientIds = new List<int>();
        var errors = new List<RecipientError>();

        foreach (var recipientId in request.RecipientIds)
        {
            Contact recipient;
            try
            {
                recipient = await _contactRepository.GetContactByIdAsync(recipientId);
            }
            catch (NotFoundException)
            {
                errors.Add(new RecipientError { RecipientId = recipientId, Reason = "RecipientNotFound" });
                continue;
            }

            if (!ContactType.Collaborator.ToString().Equals(recipient.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add(new RecipientError { RecipientId = recipientId, Reason = "RecipientNotCollaborator" });
                continue;
            }

            if (!await _delegationRequestRepository.HasRoleOnAccountAsync(recipientId, request.AccountId))
            {
                errors.Add(new RecipientError { RecipientId = recipientId, Reason = "RecipientDoesNotHaveAccess" });
                continue;
            }

            validRecipientIds.Add(recipientId);
        }

        // If no valid recipients, throw an error
        if (validRecipientIds.Count == 0)
        {
            throw new BadRequestException(Errors.RecipientDoesNotHaveAccessCode, Errors.RecipientDoesNotHaveAccessMessage);
        }

        // Create delegation requests with pending status for valid recipients
        await _delegationRequestRepository.CreateDelegationRequestsAsync(contactId, request.AccountId, validRecipientIds.ToArray(), DelegationStatusValues.Pending);

        await SendDelegationRequestNotificationsAsync(validRecipientIds, requestor, request.AccountId);

        return new CreateDelegationRequestsResponse
        {
            CreatedRecipientIds = validRecipientIds.ToArray(),
            Errors = errors
        };
    }

    public async Task<Paging<DelegationRequest>> GetSentRequestsAsync(int contactId, Pagination? pagination)
    {
        pagination ??= new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        return await _delegationRequestRepository.GetSentRequestsAsync(contactId, null, pagination);
    }

    public async Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, Pagination? pagination, DelegationRequestStatus[]? statuses)
    {
        pagination ??= new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        // Convert enums to lowercase strings; default to pending only
        var statusStrings = statuses is { Length: > 0 }
            ? statuses.Select(s => s.ToString().ToLowerInvariant()).ToArray()
            : DefaultStatuses;

        return await _delegationRequestRepository.GetReceivedRequestsAsync(contactId, statusStrings, pagination);
    }

    public async Task<DelegationEligibilityResponse> CheckEligibilityAsync(int contactId, int accountId)
    {
        // Validate inputs
        if (accountId <= 0)
        {
            throw new BadRequestException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        // Verify account exists
        if (!await _delegationRequestRepository.DoesAccountExistAsync(accountId))
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        // Check if user already has access to the account (via role or active delegation)
        if (await HasAccessToAccountAsync(contactId, accountId))
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

    public async Task<ProcessDelegationRequestsResponse> AcceptRequestsAsync(int currentUserId, AcceptDelegationRequestsRequest request)
    {
        var (validIds, errors, validRequests) = await ValidateAndGetPendingRequestIdsAsync(currentUserId, request.DelegationRequestIds);

        var respondedAt = DateTime.UtcNow;
        await _delegationRequestRepository.AcceptRequestsAsync(validIds, respondedAt);

        // Une seule validation suffit : accepter toutes les demandes sibling pending (même requester + account)
        var processedPairs = validRequests
            .Where(dr => validIds.Contains(dr.DelegationRequestId))
            .Select(dr => (dr.RequesterId, dr.AccountId))
            .Distinct()
            .ToList();

        foreach (var (requesterId, accountId) in processedPairs)
        {
            await _delegationRequestRepository.AcceptSiblingRequestsAsync(requesterId, accountId, respondedAt);
        }

        // Créer effectivement la délégation pour donner accès au demandeur
        foreach (var (requesterId, accountId) in processedPairs)
        {
            await CreateDelegationForAcceptedRequestAsync(currentUserId, requesterId, accountId);
        }

        // Notifier les collaborateurs concernés par la demande validée
        foreach (var (requesterId, accountId) in processedPairs)
        {
            var acceptedRequests = await _delegationRequestRepository.GetAcceptedSiblingRequestsAsync(requesterId, accountId, respondedAt);
            await _delegationRequestEventPublisher.PublishDelegationRequestValidatedEventAsync(currentUserId, acceptedRequests);
        }

        return new ProcessDelegationRequestsResponse
        {
            ProcessedIds = validIds,
            Errors = errors
        };
    }

    private async Task CreateDelegationForAcceptedRequestAsync(int delegatorId, int requesterId, int accountId)
    {
        var delegationRequest = new CreateDelegationRequest
        {
            DelegationDetails = new List<DelegationDetails>
            {
                new DelegationDetails
                {
                    DelegateeId = requesterId,
                    StartDate = DateTime.UtcNow,
                    IsAutomaticDelegation = false
                }
            },
            AccountIds = new List<int> { accountId },
            IsFullDelegation = false
        };

        await _delegationService.CreateDelegationAsync(delegatorId, delegationRequest);
    }

    public async Task<ProcessDelegationRequestsResponse> RefuseRequestsAsync(int currentUserId, RefuseDelegationRequestsRequest request)
    {
        var (validIds, errors, validRequests) = await ValidateAndGetPendingRequestIdsAsync(currentUserId, request.DelegationRequestIds);

        var respondedAt = DateTime.UtcNow;
        await _delegationRequestRepository.RefuseRequestsAsync(validIds, respondedAt);

        // Vérifier si tous les collaborateurs ont refusé (plus aucune demande pending pour ce requester/account)
        var processedPairs = validRequests
            .Where(dr => validIds.Contains(dr.DelegationRequestId))
            .Select(dr => (dr.RequesterId, dr.AccountId))
            .Distinct()
            .ToList();

        var allRefusedRequesterIds = new List<int>();
        foreach (var (requesterId, accountId) in processedPairs)
        {
            if (await _delegationRequestRepository.AreAllSiblingRequestsRefusedAsync(requesterId, accountId))
            {
                allRefusedRequesterIds.Add(requesterId);
                await NotifyRequesterOfRefusalAsync(currentUserId, requesterId, accountId);
            }
        }

        return new ProcessDelegationRequestsResponse
        {
            ProcessedIds = validIds,
            Errors = errors,
            AllRefusedRequesterIds = allRefusedRequesterIds.Distinct().ToArray()
        };
    }

    private async Task NotifyRequesterOfRefusalAsync(int refuserContactId, int requesterId, int accountId)
    {
        try
        {
            var refusedRequests = await _delegationRequestRepository.GetRefusedSiblingRequestsAsync(requesterId, accountId);
            if (refusedRequests.Count == 0)
            {
                return;
            }

            var refuserIds = refusedRequests.Select(dr => dr.RecipientId).Distinct().ToArray();
            var requester = await _contactRepository.GetContactByIdAsync(requesterId);
            var account = await _accountRepository.GetAccountAsync(accountId);

            await _emailService.SendDelegationRequestRefusedEmailAsync(requester, account, refuserIds);
            await _delegationRequestEventPublisher.PublishDelegationRequestRefusedEventAsync(refuserContactId, requesterId, accountId, account.AccountType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify requester {RequesterId} of the refused delegation request on account {AccountId}", requesterId, accountId);
        }
    }

    private async Task<bool> HasAccessToAccountAsync(int contactId, int accountId)
    {
        var hasRole = await _delegationRequestRepository.HasRoleOnAccountAsync(contactId, accountId);
        if (hasRole)
        {
            return true;
        }

        return await _delegationRequestRepository.HasActiveDelegationOnAccountAsync(contactId, accountId);
    }

    private async Task<(int[] ValidIds, List<DelegationRequestError> Errors, List<DelegationRequest> ValidRequests)> ValidateAndGetPendingRequestIdsAsync(int currentUserId, int[]? delegationRequestIds)
    {
        if (delegationRequestIds == null || delegationRequestIds.Length == 0)
        {
            throw new BadRequestException(Errors.DelegationRequestIdsEmptyCode, Errors.DelegationRequestIdsEmptyMessage);
        }

        var validRequests = await _delegationRequestRepository.GetPendingRequestsByIdsAndRecipientAsync(delegationRequestIds, currentUserId);

        var validIds = validRequests.Select(dr => dr.DelegationRequestId).ToArray();
        var errors = BuildErrorsForInvalidIds(delegationRequestIds, validIds);

        if (validIds.Length == 0)
        {
            throw new BadRequestException(Errors.DelegationRequestAllInvalidCode, Errors.DelegationRequestAllInvalidMessage);
        }

        return (validIds, errors, validRequests);
    }

    private static List<DelegationRequestError> BuildErrorsForInvalidIds(int[] requestedIds, int[] validIds)
    {
        return requestedIds
            .Where(id => !validIds.Contains(id))
            .Select(id => new DelegationRequestError { DelegationRequestId = id, Reason = "InvalidRequest" })
            .ToList();
    }

    private async Task SendDelegationRequestNotificationsAsync(IEnumerable<int> recipientIds, Contact requestor, int accountId)
    {
        var account = await _accountRepository.GetAccountAsync(accountId);

        await _emailService.SendDelegationRequestEmailsAsync(recipientIds, requestor, account);
        await _delegationRequestEventPublisher.PublishDelegationRequestCreatedEventAsync(account, requestor.ContactId, recipientIds);
    }
}
