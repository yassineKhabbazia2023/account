// <copyright file="IRolesRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IRoleRepository
{
    Task<Paging<Models.Account>> GetContactRolesAsync(int contactId, Pagination pagination);

    Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId);

    Task<Role?> CreateRoleAsync(CreateRoleRequest role);

    Task<Role> UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory);

    Task DeleteRoleAsync(int accountId, int contactId);

    Task<bool> CheckRoleExistsAsync(int currentUserId, int? contactId, int? accountId, string? email, bool includeProspects = false);

    Task<Role?> GetContactRoleAsync(int accountId, int contactId);

    Task<bool> IsContactHasRoleOnAccountAsync(int contactId, int? accountId, string? accountNumber);

    Task UpdateRoleCollaboratorInformationAsync(int accountId, int contactId, bool isCustomerRelation, int actionLevel);

    Task<List<Role>> GetRolesByContactAndAccountIdsAsync(int contactId, List<int> accountIds);

    Task<HashSet<int>> GetAccountIdsWithRoleLabelAsync(int contactId, List<int> accountIds);

    Task<UpdateRoleCustomerRelationResponse> UpdateRoleCustomerRelationAsync(int contactId, bool isCustomerRelation, Dictionary<int, int> accountActionLevels);

    Task<Role?> CreateRoleWithoutAccountValidationAsync(CreateRoleRequest role);

    Task<bool> IsProspectAccountAsync(int accountId);

    Task<IReadOnlyList<int>> GetAccountsWhereContactIsLastCollaboratorAsync(int contactId, IReadOnlyCollection<int> accountIds);

    Task UpdateLastActivityDateAsync(int accountId, int contactId, string contactType, DateTime lastActivityDate);
}
