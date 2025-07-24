// <copyright file="IRolesRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IRoleRepository
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, Pagination pagination);

    Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId);

    Task<IEnumerable<Role>> CreateRoleAsync(CreateRoleRequest role);

    Task<Role> UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory);

    Task DeleteRoleAsync(int accountId, int contactId);

    Task<bool> CheckRoleExistsAsync(int currentUserId, int? contactId, int? accountId, string? email);

    Task<Role> GetContactRoleAsync(int accountId, int contactId);

    Task<bool> IsContactHasRoleOnAccount(int contactId, int? accountId, string? accountNumber);

    Task UpdateRoleCollaboratorInformationAsync(int accountId, int contactId, bool isCustomerRelation, int expectedActionLevel = (int)ActionLevelType.NotAssigned);
}
