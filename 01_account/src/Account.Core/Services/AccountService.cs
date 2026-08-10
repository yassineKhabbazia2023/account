// <copyright file="AccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class AccountService(
    IAccountRepository accountRepository,
    IContactRepository contactRepository,
    IAccountEventPublisher accountEventPublisher,
    ILogger<AccountService> logger,
    IFeatureFlagService featureFlagService,
    IRoleRepository roleRepository) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IAccountEventPublisher _accountEventPublisher = accountEventPublisher;
    private readonly ILogger<AccountService> _logger = logger;
    private readonly IFeatureFlagService _featureFlagService = featureFlagService;
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<AccountDetail> CreateAccountAsync(int currentUserId, CreateAccountRequest request)
    {
        var currentUser = await contactRepository.GetContactByIdAsync(currentUserId);
        if (currentUser == null)
        {
            _logger.LogError("Utilisateur avec l'ID {UserId} non trouvé lors de la création du compte.", currentUserId);
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, currentUserId));
        }

        request.NafCode = NafCodeFormatter.Format(request.NafCode);

        try
        {
            var createdAccount = await _accountRepository.CreateAccountAsync(currentUser.Email, request);
            await _accountEventPublisher.PublishAccountCreatedEventAsync(createdAccount);

            return createdAccount;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Account creation failed. ProspectCreationStep: {ProspectCreationStep}, ServiceName: {ServiceName}, OperationName: {OperationName}, Siret: {Siret}, AccountNumber: {AccountNumber}, CurrentUserId: {CurrentUserId}",
                "CreateRydgeAccountAsync",
                "Pulse.Back.Account",
                nameof(CreateAccountAsync),
                request.Siret,
                request.AccountNumber,
                currentUserId);
            throw;
        }
    }

    public async Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination? pagination, string? currentUserEmail = null)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        criteria = criteria ?? new SearchAccountCriteria();
        criteria.DeploymentStatus = DeploymentStatusValidation.GetValidDeploymentStatuses(criteria.DeploymentStatus);
        criteria.MissionType = MissionTypeValidation.GetValidMissionTypes(criteria.MissionType);
        LastActivityRangeValidation.Validate(criteria.LastActivityDateFrom, criteria.LastActivityDateTo);

        var sortByLastActivity = await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.LastActivityFeature, currentUserEmail);

        return await _accountRepository.GetAccountsAsync(criteria, pagination, sortByLastActivity);
    }

    public async Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination? pagination, SearchAccountCriteria? criteria)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        return await _accountRepository.GetAllAccountsAsync(accountNumber, pagination, criteria ?? new SearchAccountCriteria());
    }

    public async Task<Paging<AccountSearchResult>> SearchAccountsAsync(string? keyword, Pagination? pagination)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        return await _accountRepository.SearchAccountsAsync(keyword, pagination);
    }

    public async Task<Models.Account?> GetAccountSummaryAsync(int contactId, int accountId, string contactType)
    {
        var summary = await _accountRepository.GetAccountSummaryAsync(contactId, accountId);
        await _roleRepository.UpdateLastActivityDateAsync(accountId, contactId, contactType, DateTime.UtcNow);

        return summary;
    }

    public async Task<AccountDetail?> GetAccountAsync(int accountId)
    {
        return await _accountRepository.GetAccountAsync(accountId);
    }

    public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
    {
        return await _accountRepository.GetAccountDetailAsync(accountId);
    }

    public async Task UpdateAccountAsync(int accountId, AccountDetail accountDetail, string? currentUserEmail = null)
    {
        var includeProspects = await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.IncludeProspectsInContactsSearch, currentUserEmail);

        // Récupérer l'état actuel pour vérifier les champs protégés
        var currentAccount = await _accountRepository.GetAccountAsync(accountId);
        if (currentAccount != null)
        {
            ProtectRequiredFields(currentAccount, accountDetail);
        }

        var updatedAccount = await _accountRepository.UpdateAccountAsync(accountId, accountDetail, includeProspects);
        await _accountEventPublisher.PublishAccountUpdatedEventAsync(updatedAccount);
    }

    private void ProtectRequiredFields(AccountDetail currentAccount, AccountDetail accountDetail)
    {
        // Protéger StaffSizeRange
        if (!string.IsNullOrWhiteSpace(currentAccount.Legal?.StaffSizeRange)
            && string.IsNullOrWhiteSpace(accountDetail.Legal?.StaffSizeRange))
        {
            _logger.LogError(
                "Tentative de suppression du champ StaffSizeRange pour le compte {AccountId}. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.",
                currentAccount.AccountId,
                currentAccount.Legal.StaffSizeRange);

            if (accountDetail.Legal != null)
            {
                accountDetail.Legal.StaffSizeRange = currentAccount.Legal.StaffSizeRange;
            }
        }

        // Protéger AccountingType
        if (!string.IsNullOrWhiteSpace(currentAccount.Accounting?.AccountingType)
            && string.IsNullOrWhiteSpace(accountDetail.Accounting?.AccountingType))
        {
            _logger.LogError(
                "Tentative de suppression du champ AccountingType pour le compte {AccountId}. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.",
                currentAccount.AccountId,
                currentAccount.Accounting.AccountingType);

            if (accountDetail.Accounting != null)
            {
                accountDetail.Accounting.AccountingType = currentAccount.Accounting.AccountingType;
            }
        }
    }

    public async Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination? pagination, string? currentUserEmail = null)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        criteria = criteria ?? new SearchContactsAccountCriteria();
        var includeProspects = await _featureFlagService.IsEnabledAsync(FeatureFlagKeys.IncludeProspectsInContactsSearch, currentUserEmail);
        return await _accountRepository.GetContactsAccountAsync(accountId, criteria, pagination, includeProspects);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Contact>> GetAccountContactWidgetContactsAsync(int accountId)
    {
        return await _accountRepository.GetAccountContactWidgetContactsAsync(accountId);
    }

    /// <inheritdoc/>
    public async Task<bool> IsContactProspectOnlyAsync(int contactId)
    {
        return await _accountRepository.IsContactProspectOnlyAsync(contactId);
    }

    public async Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination? pagination)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);
        return await _accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);
    }
}
