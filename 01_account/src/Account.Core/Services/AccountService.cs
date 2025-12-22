// <copyright file="AccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountEventPublisher _accountEventPublisher;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            IAccountEventPublisher accountEventPublisher,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _accountEventPublisher = accountEventPublisher;
            _logger = logger;
        }

        public async Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination? pagination)
        {
            pagination = pagination ?? new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

            criteria = criteria ?? new SearchAccountCriteria();
            criteria.DeploymentStatus = DeploymentStatusValidation.GetValidDeploymentStatus(criteria.DeploymentStatus);

            return await _accountRepository.GetAccountsAsync(criteria, pagination);
        }

        public async Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination? pagination, SearchAccountCriteria? criteria)
        {
            pagination = pagination ?? new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

            return await _accountRepository.GetAllAccountsAsync(accountNumber, pagination, criteria ?? new SearchAccountCriteria());
        }

        public async Task<Models.Account?> GetAccountSummaryAsync(int contactId, int accountId)
        {
            return await _accountRepository.GetAccountSummaryAsync(contactId, accountId);
        }

        public async Task<AccountDetail?> GetAccountAsync(int accountId)
        {
            return await _accountRepository.GetAccountAsync(accountId);
        }

        public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
        {
            return await _accountRepository.GetAccountDetailAsync(accountId);
        }

        public async Task UpdateAccountAsync(int accountId, AccountDetail accountDetail)
        {
            // Récupérer l'état actuel pour vérifier les champs protégés
            var currentAccount = await _accountRepository.GetAccountAsync(accountId);
            if (currentAccount != null)
            {
                ProtectRequiredFields(currentAccount, accountDetail);
            }

            var updatedAccount = await _accountRepository.UpdateAccountAsync(accountId, accountDetail);
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

        public async Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination? pagination)
        {
            pagination = pagination ?? new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

            criteria = criteria ?? new SearchContactsAccountCriteria();
            return await _accountRepository.GetContactsAccountAsync(accountId, criteria, pagination);
        }

        public async Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination? pagination)
        {
            pagination = pagination ?? new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);
            return await _accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);
        }
    }
}
