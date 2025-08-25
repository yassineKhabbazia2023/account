// <copyright file="IHistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IHistoryEventPublisher
{
  Task PublishHistoryCreatedEventAsync(int currentUserId, int contactId, int accountId);
}
