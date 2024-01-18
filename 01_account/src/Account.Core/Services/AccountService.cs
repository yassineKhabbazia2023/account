// <copyright file="AccountService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Linq;
using System.Text.Json;
using Kpmg.Account.Core.Models;
using Pulse.Account.Core.Interfaces;

namespace Kpmg.Offer.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public Paging<Account.Core.Models.Account> GetAccountList(string search, int page, int limit)
        {
            try
            {
                string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
                var accountList = JsonSerializer.Deserialize<Paging<Account.Core.Models.Account>>(accountMocked, _jsonOptions) ?? new Paging<Account.Core.Models.Account>();
                accountList.Items = accountList.Items?
                    .Where(item => !string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(item.LegalName) ? item.LegalName.Contains(search, StringComparison.OrdinalIgnoreCase) : string.IsNullOrEmpty(search))
                    .Skip((page - 1) * limit)
                    .Take(limit);
                return accountList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public AccountDetail GetAccountDetail(Guid id)
        {
            try
            {
                string accountDetailMocked = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
                var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountDetailMocked, _jsonOptions) ?? new AccountDetail();
                return accountDetail;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public AccountDetail UpdateAccount(Guid id, AccountDetail updatedAccount)
        {
            try
            {
                // TODO: update account here
                return this.GetAccountDetail(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<AccountFavorite> GetAccountFavoriteList(string token)
        {
            try
            {
                string accountFavoriteMocked = File.ReadAllText(@"./MockedResponses/AccountFavoriteMocked.json");
                var accountFavoriteList = JsonSerializer.Deserialize<List<AccountFavorite>>(accountFavoriteMocked, _jsonOptions) ?? new List<AccountFavorite>();
                return accountFavoriteList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetFavorite(Guid id, bool isFavorite, string token)
        {
            try
            {
                // TODO: put set favorite here
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
