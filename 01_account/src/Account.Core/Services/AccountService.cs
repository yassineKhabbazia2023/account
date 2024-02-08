// <copyright file="AccountService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Kpmg.Account.Core.Interfaces;
using Kpmg.Account.Core.Models;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit, int contactId)
        {
            page = page == 0 ? 1 : page;
            limit = limit == 0 ? int.MaxValue : limit;
            var accountList = await _accountRepository.GetAccountsAsync(search, page, limit, contactId);
            return accountList;
        }

        public async Task<AccountDetail> GetAccountDetailAsync(int id)
        {
            var accountDetail = await _accountRepository.GetAccountDetailAsync(id);
            return accountDetail;
        }

        public async Task<AccountDetail> UpdateAccountAsync(int id, AccountDetail accountDetail)
        {
            return await _accountRepository.UpdateAccountAsync(accountDetail, id);
        }

        public IReadOnlyCollection<AccountFavorite> GetAccountFavoritesAsync(int contactId)
        {
            string accountFavoriteMocked = File.ReadAllText(@"./MockedResponses/AccountFavoriteMocked.json");
            var accountFavoriteList = JsonSerializer.Deserialize<IReadOnlyCollection<AccountFavorite>>(accountFavoriteMocked, _jsonOptions);
            return accountFavoriteList ?? new List<AccountFavorite>();
        }

        public void SetFavoriteAsync(int accountId, int contactId, bool isFavorite)
        {
            // implement set favorite function
        }
    }
}
