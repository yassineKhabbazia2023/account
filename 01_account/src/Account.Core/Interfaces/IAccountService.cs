// <copyright file="IAccountService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountService
    {
        public Task<Paging<Models.Account>> GetAccountsAsync(string? search, int page, int limit, int contactId);

        public Task<AccountDetail?> GetAccountDetailAsync(int id);

        public Task<AccountDetail?> UpdateAccountAsync(int id, AccountDetail accountDetail);

        public Task<IEnumerable<AccountFavorite>> GetAccountFavoritesAsync(int contactId);

        public Task SetFavoriteAsync(int accountId, int contactId, bool isFavorite);

        Task<Statistics> GetStatisticsAsync(int contactId);
    }
}
