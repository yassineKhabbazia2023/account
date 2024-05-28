// <copyright file="IDelegationEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IDelegationEventRepository
{
    Task DeleteContactDelegationsAsync(int contactId);
}
