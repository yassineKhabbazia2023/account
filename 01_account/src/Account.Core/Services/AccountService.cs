// <copyright file="AccountService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
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

        public async Task<Paging<Models.Account>> GetAccountsAsync(string? search, int page, int limit, int contactId)
        {
            page = page == 0 ? 1 : page;
            limit = limit == 0 ? int.MaxValue : limit;
            return await _accountRepository.GetAccountsAsync(search, page, limit, contactId);
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
    }
}
