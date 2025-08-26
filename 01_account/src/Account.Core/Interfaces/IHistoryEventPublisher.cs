// <copyright file="IHistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IHistoryEventPublisher
{
    Task PublishHistoryCreatedEventAsync(string registryApproverEmail, int contactId, int accountId);

    Task PublishHistoryCreatedEventAsync(int currentUserId, int contactId, int accountId, string actionCode);
}
