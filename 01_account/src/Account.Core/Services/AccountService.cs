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
        private IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            this._accountRepository = accountRepository;
        }

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit)
        {
            page = page == 0 ? 1 : page;
            limit = limit == 0 ? int.MaxValue : limit;
            var accountList = await this._accountRepository.GetAccountsAsync(search, page, limit);
            return accountList;
        }

        public AccountDetail GetAccountDetailAsync(Guid id)
        {
            string accountDetailMocked = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountDetailMocked, _jsonOptions) ?? new AccountDetail();
            return accountDetail;
        }

        public AccountDetail UpdateAccountAsync(Guid id, AccountDetail accountDetail)
        {
            // TODO: update account here
            return this.GetAccountDetailAsync(id);
        }

        public IReadOnlyCollection<AccountFavorite> GetAccountFavoritesAsync(Guid contactId)
        {
            string accountFavoriteMocked = File.ReadAllText(@"./MockedResponses/AccountFavoriteMocked.json");
            var accountFavoriteList = JsonSerializer.Deserialize<IReadOnlyCollection<AccountFavorite>>(accountFavoriteMocked, _jsonOptions);
            return accountFavoriteList ?? new List<AccountFavorite>();
        }

        public void SetFavoriteAsync(Guid accountId, Guid contactId, bool isFavorite)
        {
            // implement set favorite function
        }
    }
}
