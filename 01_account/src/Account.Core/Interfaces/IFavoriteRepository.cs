// <copyright file="IFavoriteRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId);

        Task UpdateAccountFavoriteAsync(int accountId, int contactId, bool isFavorite);
    }
}
