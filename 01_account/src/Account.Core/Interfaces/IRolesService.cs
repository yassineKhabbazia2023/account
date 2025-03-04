// <copyright file="IRolesService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IRolesService
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, Pagination? pagination);

    Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId);

    Task CreateRoleAsync(CreateRoleRequest role, int contactId);

    Task UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory);

    Task DeleteRoleAsync(int accountId, int contactId);

    Task<bool> CheckRoleExistsAsync(int contactId, int? accountId, string email);

    Task<bool> IsContactHasRoleOnAccount(int contactId, int? accountId, string? accountNumber);
}
