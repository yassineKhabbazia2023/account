// <copyright file="IRolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces;

public interface IRolesService
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, int page, int limit);

    Task<IEnumerable<Signatory>> GetSignatoryAsync(int accountId);
}
