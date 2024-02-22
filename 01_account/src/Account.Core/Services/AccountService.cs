// <copyright file="AccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Paging<Models.Account>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId)
        {
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            pageSize = pageSize == 0 ? int.MaxValue : pageSize;
            return await _accountRepository.GetAccountsAsync(search, pageNumber, pageSize, contactId);
        }

        public async Task<AccountDetail?> GetAccountDetailAsync(int id)
        {
            return await _accountRepository.GetAccountDetailAsync(id);
        }

        public async Task<AccountDetail?> UpdateAccountAsync(int id, AccountDetail accountDetail)
        {
            return await _accountRepository.UpdateAccountAsync(accountDetail, id);
        }

        public async Task<Statistics> GetStatisticsAsync(int contactId)
        {
            return await _accountRepository.GetStatisticsAsync(contactId);
        }

        public async Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId)
        {
            return await _accountRepository.GetContactsAccountAsync(accountId);
        }
    }
}
