// <copyright file="IFavoriteRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId);

        Task UpdateAccountFavoriteAsync(int accountId, int contactId, bool isFavorite);
    }
}
