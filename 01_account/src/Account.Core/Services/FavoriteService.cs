// <copyright file="FavoriteService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Microsoft.Extensions.Logging;

namespace Pulse.Account.Core.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IRoleEventPublisher _roleEventPublisher;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(IFavoriteRepository favoriteRepository,
            IRoleEventPublisher roleEventPublisher,
            ILogger<FavoriteService> logger)
        {
            _favoriteRepository = favoriteRepository;
            _roleEventPublisher = roleEventPublisher;
            _logger = logger;
        }

        public async Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId)
        {
            return await _favoriteRepository.GetAccountFavoritesByContactIdAsync(contactId);
        }

        public async Task SetFavoriteAsync(int accountId, int contactId, bool isFavorite)
        {
            await _favoriteRepository.UpdateAccountFavoriteAsync(accountId, contactId, isFavorite);

            await PublishRoleUpdatedEvent(accountId, contactId, isFavorite);
        }

        private async Task PublishRoleUpdatedEvent(int accountId, int contactId, bool isFavorite)
        {
            _logger.LogInformation("RoleService: Start send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);

            await _roleEventPublisher.PublishRoleFavoriteStatusChangedEventAsync(accountId, contactId, isFavorite);

            _logger.LogInformation("RoleService: End send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
        }
    }
}
