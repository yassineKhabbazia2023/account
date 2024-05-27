// <copyright file="IAccountEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountEventPublisher
    {
        public Task PublishAccountCreatedEventAsync(AccountDetail account);

        public Task PublishAccountUpdatedEventAsync(AccountDetail account);

        public Task PublishAccountRemovedEventAsync(int accountId);
    }
}
