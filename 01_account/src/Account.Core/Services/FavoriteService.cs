// <copyright file="FavoriteService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Microsoft.Extensions.Logging;

namespace Pulse.Account.Core.Services;

public class FavoriteService(IFavoriteRepository favoriteRepository,
    IRoleEventPublisher roleEventPublisher,
    ILogger<FavoriteService> logger,
    IRoleRepository roleRepository) : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository = favoriteRepository;
    private readonly IRoleEventPublisher _roleEventPublisher = roleEventPublisher;
    private readonly ILogger<FavoriteService> _logger = logger;
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<IEnumerable<AccountFavorite>> GetAccountFavoritesByContactIdAsync(int contactId)
    {
        return await _favoriteRepository.GetAccountFavoritesByContactIdAsync(contactId);
    }

    public async Task SetFavoriteAsync(int accountId, int contactId, string contactType, bool isFavorite)
    {
        await _favoriteRepository.UpdateAccountFavoriteAsync(accountId, contactId, isFavorite);
        await _roleRepository.UpdateLastActivityDateAsync(accountId, contactId, contactType, DateTime.UtcNow);

        await PublishRoleUpdatedEvent(accountId, contactId);
    }

    private async Task PublishRoleUpdatedEvent(int accountId, int contactId)
    {
        _logger.LogInformation("RoleService: Start send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);

        await _roleEventPublisher.PublishRoleUpdatedEventAsync(accountId, contactId);

        _logger.LogInformation("RoleService: End send update role event. AccountId : {accountId} - ContactId : {contactId}", accountId, contactId);
    }
}
