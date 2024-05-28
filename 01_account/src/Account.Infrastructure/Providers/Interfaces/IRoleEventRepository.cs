// <copyright file="IRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IRoleEventRepository
{
    Task DeleteContactRolesAsync(int contactId);
}
