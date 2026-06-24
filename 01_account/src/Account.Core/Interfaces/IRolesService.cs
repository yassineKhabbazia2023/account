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

    Task CreateRoleAsync(CreateRoleRequest role, int currentUserId);

    Task<CreateRolesBulkResult> CreateRolesBulkAsync(int accountId, CreateRolesBulkRequest request, int currentUserId);

    Task UpdateRoleSignatoryAsync(int currentUserId, int accountId, int contactId, bool isSignatory);

    Task DeleteRoleAsync(int currentUserId, int accountId, int contactId);

    Task<BulkRoleDeleteResult> BulkDeleteRolesAsync(int currentUserId, BulkRoleDeleteRequest request);

    Task<LastCollaboratorCheckResult> CheckLastCollaboratorAsync(int contactId, IReadOnlyCollection<int> accountIds);

    Task<bool> CheckRoleExistsAsync(int currentUserId, int? contactId, int? accountId, string? email);

    Task<bool> IsContactHasRoleOnAccount(int contactId, int? accountId, string? accountNumber);

    Task UpdateRoleCustomerRelationAsync(int accountId, int contactId, bool isCustomerRelation);
}
