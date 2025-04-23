// <copyright file="IRoleEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces
{
    public interface IRoleEventPublisher
    {
        Task PublishRoleCreatedEventAsync(CreateRoleRequest roleRequest);

        Task PublishRoleDeletedEventAsync(int accountId, int contactId);

        Task PublishRoleUpdatedEventAsync(int accountId, int contactId, bool isSignatory);

        Task PublishRoleFavoriteStatusChangedEventAsync(int accountId, int contactId, bool isFavorite);
    }
}
