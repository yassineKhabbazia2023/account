// <copyright file="IRoleEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces
{
    public interface IRoleEventPublisher
    {
        Task PublishRoleCreatedEventAsync(CreateRoleRequest roleRequest, string? subRole = null, bool includeProspects = false);

        Task PublishRoleDeletedEventAsync(int accountId, int contactId, bool includeProspects = false);

        Task PublishRoleUpdatedEventAsync(int accountId, int contactId);
    }
}
