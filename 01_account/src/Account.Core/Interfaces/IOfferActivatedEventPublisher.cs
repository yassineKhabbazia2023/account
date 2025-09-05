// <copyright file="IOfferActivatedEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IOfferActivatedEventPublisher
{
    Task PublishOfferActivatedEventAsync(int accountId, string offerName);
}
