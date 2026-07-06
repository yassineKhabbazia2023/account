// <copyright file="IFavoriteService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IFavoriteService
    {
        public Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId);

        public Task SetFavoriteAsync(int accountId, int contactId, string contactType, bool isFavorite);
    }
}
