// <copyright file="AccountService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Kpmg.Account.Core.Models;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public Paging<AccountModel> GetAccountsAsync(string? search, int page, int limit)
        {
            page = page == 0 ? 1 : page;
            limit = limit == 0 ? int.MaxValue : limit;
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            accountList.Items = accountList.Items?
                .Where(item => !string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(item.LegalName) ? item.LegalName.Contains(search, StringComparison.OrdinalIgnoreCase) : string.IsNullOrEmpty(search))
                .Skip((page - 1) * limit)
                .Take(limit);
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
            return accountFavoriteList;
        }

        public void SetFavoriteAsync(Guid accountId, Guid contactId, bool isFavorite)
        {
        }
    }
}
