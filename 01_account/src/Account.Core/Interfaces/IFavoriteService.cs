// <copyright file="IFavoriteService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IFavoriteService
    {
        public Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId);

        public Task SetFavoriteAsync(int accountId, int contactId, bool isFavorite);
    }
}
