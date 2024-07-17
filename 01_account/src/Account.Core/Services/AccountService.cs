// <copyright file="AccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountEventPublisher _accountEventPublisher;

        public AccountService(IAccountRepository accountRepository, IAccountEventPublisher accountEventPublisher)
        {
            _accountRepository = accountRepository;
            _accountEventPublisher = accountEventPublisher;
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

        public async Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination? pagination)
        {
            pagination = pagination ?? new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

            return await _accountRepository.GetAllAccountsAsync(accountNumber, pagination);
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
            var updatedAccount = await _accountRepository.UpdateAccountAsync(accountId, accountDetail);
            await _accountEventPublisher.PublishAccountUpdatedEventAsync(updatedAccount);
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
