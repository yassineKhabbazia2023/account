// <copyright file="RoleRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RoleRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<Paging<Core.Models.Account>> GetContactRolesAsync(int contactId, Pagination pagination)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!_accountContext.ContactEntity.Any(x => x.ContactId == contactId))
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            IQueryable<AccountEntity> query = _accountContext.AccountEntity
                                                        .AsNoTracking()
                                                        .Include(x => x.RoleEntity)
                                                        .ThenInclude(r => r.Contact)
                                                        .Include(a => a.AddressEntity)
                                                        .Include(x => x.DeploymentEntity)
                                                        .Where(a => a.RoleEntity.Any(r => r.ContactId == contactId))
                                                        .OrderBy(x => x.LegalName);

            var totalRows = await query.CountAsync();

            query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
            query = query.Take(pagination.PageSize);

            var totalPages = Paginator.GetTotalPages(totalRows, pagination.PageSize);
            var entities = await query.ToListAsync();

            return MapAccountDbToAccountModel.MapToPaginAccounts(
                  entities,
                  contactId,
                  pagination.PageNumber,
                  totalRows,
                  totalPages);
        });
    }

    public async Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!_accountContext.AccountEntity.Any(x => x.AccountId == accountId))
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
            }

            var result = await _accountContext.RoleEntity
                .AsNoTracking()
                .Include(x => x.Contact)
                .Where(x => x.AccountId == accountId && x.IsSignatory!.Value)
                .ToListAsync();

            return result.MapToContacts();
        });
    }

    public async Task<IReadOnlyList<int>> GetAccountsWhereContactIsLastCollaboratorAsync(int contactId, IReadOnlyCollection<int> accountIds)
    {
        return await _retryPolicy.ExecuteAsync<IReadOnlyList<int>>(async () =>
        {
            var collaborator = ContactType.Collaborator.ToString();

            return await _accountContext.RoleEntity
                .AsNoTracking()
                .Where(role => accountIds.Contains(role.AccountId) && role.Contact.Type == collaborator)
                .GroupBy(role => role.AccountId)
                .Where(accountGroup => accountGroup.Count() == 1 && accountGroup.Min(role => role.ContactId) == contactId)
                .Select(accountGroup => accountGroup.Key)
                .ToListAsync();
        });
    }

    public async Task<Role?> GetContactRoleAsync(int accountId, int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var role = await _accountContext.RoleEntity
                .FirstOrDefaultAsync(r => r.AccountId == accountId && r.ContactId == contactId);

            return role!.MapToRole();
        });
    }

    public async Task<Role?> CreateRoleAsync(CreateRoleRequest role)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(role.Email))
            {
                var contactId = _accountContext.ContactEntity
                        .AsNoTracking()
                        .Where(c => c.Email.Equals(role.Email))
                        .Select(c => c.ContactId)
                        .FirstOrDefault();

                role.ContactId = contactId;
            }

            if (!await _accountContext.ActiveAccounts.AnyAsync(x => x.AccountId == role.AccountId))
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, role.AccountId));
            }

            return await CreateRoleInternalAsync(role);
        });
    }

    public async Task<Role?> CreateRoleWithoutAccountValidationAsync(CreateRoleRequest role)
    {
        return await _retryPolicy.ExecuteAsync(() => CreateRoleInternalAsync(role));
    }

    private static void SetIsCustomerRelationAndActionLevel(CreateRoleRequest role, string contactType)
    {
        if (ContactType.Collaborator.ToString().Equals(contactType))
        {
            role.IsCustomerRelation = role.IsCustomerRelation.HasValue ? role.IsCustomerRelation : false;
            role.ActionLevel = role.ActionLevel.HasValue ? role.ActionLevel : (int)ActionLevelType.Observator;
        }
        else
        {
            role.IsCustomerRelation = null;
            role.ActionLevel = null;
        }
    }

    /// <summary>
    /// Creates a role after the caller-specific preconditions have been validated.
    /// </summary>
    /// <param name="role">The role creation request.</param>
    /// <returns>The created role.</returns>
    private async Task<Role?> CreateRoleInternalAsync(CreateRoleRequest role)
    {
        var contact = await _accountContext.ContactEntity.FirstOrDefaultAsync(x => x.ContactId == role.ContactId)
            ?? throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, role.ContactId));

        if (await GetContactRoleAsync(role.AccountId, (int)role.ContactId!) != null)
        {
            throw new ConflictException(Errors.BadRequestExistingRoleCode, string.Format(Errors.BadRequestExistingRoleMessage, role.ContactId, role.AccountId));
        }

        SetIsCustomerRelationAndActionLevel(role, contact.Type);

        var roleEntity = role.MapRoleToRoleDb();

        if (roleEntity != null)
        {
            await _accountContext.RoleEntity.AddRangeAsync(roleEntity);
        }

        await _accountContext.SaveChangesAsync();

        return roleEntity!.MapToRole();
    }

    public async Task<Role> UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var roles = from r in _accountContext.RoleEntity
                        where r.AccountId.Equals(accountId) && r.ContactId.Equals(contactId)
                        select r;

            var role = await roles.FirstOrDefaultAsync()
                ?? throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));

            if (role.IsSignatory != isSignatory)
            {
                role.IsSignatory = isSignatory;
                _accountContext.RoleEntity.Update(role);
                await _accountContext.SaveChangesAsync();
            }

            return role.MapToRole() !;
        });
    }

    public async Task UpdateRoleCollaboratorInformationAsync(int accountId, int contactId, bool isCustomerRelation, int actionLevel)
    {
        var roleDb = await _accountContext.RoleEntity.FirstOrDefaultAsync(role => role.AccountId == accountId && role.ContactId == contactId)
            ?? throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));

        roleDb.ActionLevel = actionLevel;
        roleDb.IsCustomerRelation = isCustomerRelation;
        await _accountContext.SaveChangesAsync();
    }

    public async Task<List<Role>> GetRolesByContactAndAccountIdsAsync(int contactId, List<int> accountIds)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var roles = await _accountContext.RoleEntity
                .AsNoTracking()
                .Where(role => role.ContactId == contactId && accountIds.Contains(role.AccountId))
                .ToListAsync();

            return roles.Select(r => r.MapToRole()).OfType<Role>().ToList();
        });
    }

    public async Task<HashSet<int>> GetAccountIdsWithRoleLabelAsync(int contactId, List<int> accountIds)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.RoleLabelEntity
                .AsNoTracking()
                .Where(rl => rl.ContactId == contactId && accountIds.Contains(rl.AccountId))
                .Select(rl => rl.AccountId)
                .ToHashSetAsync();
        });
    }

    public async Task<UpdateRoleCustomerRelationResponse> UpdateRoleCustomerRelationAsync(int contactId, bool isCustomerRelation, Dictionary<int, int> accountActionLevels)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var accountIds = accountActionLevels.Keys.ToList();

            var roles = await _accountContext.RoleEntity
                .Where(role => role.ContactId == contactId && accountIds.Contains(role.AccountId))
                .ToListAsync();

            var result = new UpdateRoleCustomerRelationResponse();

            foreach (var (accountId, actionLevel) in accountActionLevels)
            {
                var roleDb = roles.FirstOrDefault(role => role.AccountId == accountId);

                if (roleDb is null)
                {
                    result.Failed.Add(new UpdateRoleCustomerRelationItemResponse
                    {
                        AccountId = accountId,
                        ErrorCode = Errors.NotFoundRoleCode,
                        ErrorMessage = string.Format(Errors.NotFoundRoleMessage, contactId, accountId)
                    });
                    continue;
                }

                roleDb.ActionLevel = actionLevel;
                roleDb.IsCustomerRelation = isCustomerRelation;

                result.Succeeded.Add(new UpdateRoleCustomerRelationItemResponse { AccountId = accountId });
            }

            await _accountContext.SaveChangesAsync();

            return result;
        });
    }

    public async Task DeleteRoleAsync(int accountId, int contactId)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var role = _accountContext.RoleEntity
                .FirstOrDefault(r => r.AccountId == accountId && r.ContactId == contactId);

            _accountContext.Remove(role!);
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task<bool> CheckRoleExistsAsync(int currentUserId, int? contactId, int? accountId, string? email, bool includeProspects = false)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactToCheck = _accountContext.ContactEntity.FirstOrDefault(x => contactId != null ? x.ContactId == contactId : x.Email.Contains(email!));

            if (contactToCheck is null)
            {
                return false;
            }

            IQueryable<AccountEntity> accountSet = includeProspects
                ? _accountContext.ActiveAccounts
                : _accountContext.AccountEntity;

            IQueryable<AccountEntity> query = accountSet
                                                    .AsNoTracking()
                                                    .Where(a => a.RoleEntity.Any(r => r.ContactId == currentUserId))
                                                    .Where(a => a.RoleEntity.Any(r => r.ContactId == contactToCheck.ContactId));

            if (accountId is not null)
            {
                query = query.Where(a => a.AccountId == accountId);
            }

            return await query.AnyAsync();
        });
    }

    public async Task<bool> IsContactHasRoleOnAccount(int contactId, int? accountId, string? accountNumber)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.RoleEntity
                                                    .Include(x => x.Account)
                                                    .IgnoreQueryFilters()
                                                    .AsNoTracking()
                                                    .AnyAsync(r => r.ContactId == contactId &&
                                                                (accountId.HasValue ?
                                                                r.AccountId == accountId :
                                                                r.Account.IsActive && r.Account.AccountNumber == accountNumber));
        });
    }

    public async Task<bool> IsProspectAccountAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.ActiveAccounts
                .AnyAsync(a => a.AccountId == accountId && a.AccountType == GlobalConstants.ProspectAccountType);
        });
    }

    private async Task<bool> HasRoleLabel(int contactId, int accountId)
    {
        return await _accountContext.RoleLabelEntity.AnyAsync(rl => rl.ContactId == contactId && rl.AccountId == accountId);
    }
}
