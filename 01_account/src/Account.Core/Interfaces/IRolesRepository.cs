// <copyright file="IRolesRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces;

public interface IRolesRepository
{
    Task<Paging<Pulse.Account.Core.Models.Account>> GetContactRolesAsync(int contactId, int page, int limit);
}
