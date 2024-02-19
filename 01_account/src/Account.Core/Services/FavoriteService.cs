// <copyright file="FavoriteService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteService(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId)
        {
            return await _favoriteRepository.GetAccountFavoritesByContactIdAsync(contactId);
        }

        public async Task SetFavoriteAsync(int accountId, int contactId, bool isFavorite)
        {
            await _favoriteRepository.UpdateAccountFavoriteAsync(accountId, contactId, isFavorite);
        }
    }
}
