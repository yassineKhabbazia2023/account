// <copyright file="AccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
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

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
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
            await _accountRepository.UpdateAccountAsync(accountId, accountDetail);
        }

        public async Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId, ContactType? type)
        {
            return await _accountRepository.GetContactsAccountAsync(accountId, type);
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
