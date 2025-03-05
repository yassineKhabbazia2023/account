// <copyright file="DelegationRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Utils;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class DelegationRepository : IDelegationRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public DelegationRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _retryPolicy = Policy.Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(3000));
    }

    public async Task<IEnumerable<CreateRoleRequest>> CreateDelegationAsync(CreateDelegationRequest delegation, IEnumerable<CreateRoleRequest> roles)
    {
        if (delegation == default(CreateDelegationRequest))
        {
            throw new BadRequestException(Errors.NullDelegationRequestCode, Errors.NullDelegationRequestMessage);
        }

        var contactsToCheck = delegation.DelegationDetails.Select(d => d.DelegateeId).ToList();
        contactsToCheck.Add(delegation.DelegatorId);

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!(await CheckExistingContactsAsync(contactsToCheck))?.Any() == false)
            {
                throw new NotFoundException(Errors.NotFoundContactsCode, Errors.NotFoundContactsMessage);
            }

            if (!(await CheckExistingAccountsAsync(delegation.AccountIds!))?.Any() == false)
            {
                throw new NotFoundException(Errors.NotFoundAccountsCode, string.Format(Errors.NotFoundAccountsMessage, string.Join(',', delegation.AccountIds)));
            }

            if (!await CheckContactHasAccounts(delegation.DelegatorId, delegation.AccountIds!))
            {
                throw new ExceptionMiddleware.Exceptions.InvalidOperationException(Errors.DontHaveRightAccountsCode, Errors.DontHaveRightAccountsMessage);
            }

            var accounts = await GetAccounts(delegation.AccountIds!);
            var delegationEntities = delegation.MapDelegationRequestToDelegationsDb(accounts.ToList());
            var roleEntities = await RemoveDuplicateRoles(roles.MapRolesToRoleDb());

            if (roleEntities.Any())
            {
                await _accountContext.RoleEntity.AddRangeAsync(roleEntities);
            }

            await _accountContext.DelegationEntity.AddRangeAsync(delegationEntities);
            await _accountContext.SaveChangesAsync();

            return roleEntities.ToCreateRoleRequests(delegation.DelegatorId);
        });
    }

    public async Task<IReadOnlyCollection<Delegation>> GetContactDelegationsAsync(int delegateeId)
    {
        var delegationList = new List<DelegationEntity>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegationList = await _accountContext.DelegationEntity
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.DelegateeId == delegateeId && !d.Status.Equals(DelegationStatus.Disabled.ToString()))
                                        .ToListAsync();
        });

        return delegationList.ToDelegations();
    }

    public async Task<IReadOnlyCollection<Delegation>> GetDelegationsAsync(int delegatorId, int delegateeId)
    {
        var delegations = new List<DelegationEntity>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            delegations = await _accountContext.DelegationEntity
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.DelegatorId == delegatorId && d.DelegateeId == delegateeId && !d.Status.Equals(DelegationStatus.Disabled.ToString()))
                                        .ToListAsync();
        });

        return delegations.ToDelegations();
    }

    public async Task<IEnumerable<Role>> DeleteDelegationAsync(int delegationId)
    {
        var delegationEntity = await GetDelegationAsync(delegationId);
        delegationEntity.Status = DelegationStatus.Disabled.ToString().ToLower();
        var roles = Enumerable.Empty<Role>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            _accountContext.DelegationEntity.Update(delegationEntity);

            var roleEntities = await GetRolesForDelegationAsync(delegationEntity);

            if (roleEntities.Any())
            {
                roles = roleEntities.MapToRoles();
                _accountContext.RoleEntity.RemoveRange(roleEntities);
            }

            await _accountContext.SaveChangesAsync();
        });

        return roles;
    }

    public async Task<Paging<Delegation>> GetAccountDelegationsHistoryAsync(int accountId, string? search, Pagination pagination)
    {
        var delegations = new List<DelegationEntity>();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<DelegationEntity> query = _accountContext.DelegationEntity
                                        .AsNoTracking()
                                        .Include(d => d.Account)
                                        .Include(d => d.Delegator)
                                        .Include(d => d.Delegatee)
                                        .Where(d => d.Account.Any(a => a.AccountId == accountId))
                                        .OrderByDescending(d => d.CreationDate);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = from d in query
                        where d.Delegator.LastName.Contains(search) ||
                        d.Delegatee.LastName.Contains(search) ||
                        d.Delegator.FirstName.Contains(search) ||
                        d.Delegatee.FirstName.Contains(search)
                        select d;
            }

            var totalItems = await query.CountAsync();
            var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

            query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
            query = query.Take(pagination.PageSize);

            delegations = await query.ToListAsync();

            return delegations.MapToPagingDelegations(pagination.PageNumber, totalItems, totalPages);
        });
    }

    public async Task<Paging<Delegation>> GetContactDelegationsHistoryAsync(int contactId, Pagination pagination, bool sortAscending)
    {
        var delegations = new List<DelegationEntity>();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<DelegationEntity> query = _accountContext.DelegationEntity
                                    .AsNoTracking()
                                        .Include(d => sortAscending
                                            ? d.Account.OrderBy(a => a.LegalName)
                                            : d.Account.OrderByDescending(a => a.LegalName))
                                        .Include(d => d.Delegatee)
                                        .Include(d => d.Delegator)
                                        .Where(d => d.DelegateeId == contactId || d.DelegatorId == contactId)
                                        .OrderByDescending(d => d.CreationDate);

            var totalItems = await query.CountAsync();
            var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

            query = query.Skip((pagination.PageNumber - 1) * pagination.PageSize);
            query = query.Take(pagination.PageSize);

            delegations = await query.ToListAsync();

            return delegations.MapToPagingDelegations(pagination.PageNumber, totalItems, totalPages);
        });
    }

    public async Task<bool> DoesAccountExistAsync(int accountId)
    {
        var validAccount = false;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            validAccount = await _accountContext.AccountEntity.AnyAsync(d => d.AccountId == accountId);
        });

        return validAccount;
    }

    public async Task<bool> DoesContactExistAsync(int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.ContactEntity.AnyAsync(c => c.ContactId == contactId);
        });
    }

    public async Task<IEnumerable<int>> GetAccountIdsForFullDelegationAsync(int delegatorId)
    {
        var accountIds = Enumerable.Empty<int>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            accountIds = await _accountContext.RoleEntity
                            .Where(r => r.ContactId == delegatorId)
                            .Include(r => r.Contact)
                            .Where(r => r.Contact.IsActive)
                            .Include(r => r.Account)
                            .ThenInclude(a => a.DeploymentEntity)
                            .AsNoTracking()
                            .Select(a => a.AccountId)
                            .ToListAsync();
        });

        return accountIds;
    }

    private async Task<IEnumerable<int>> CheckExistingContactsAsync(IEnumerable<int> contactIds)
    {
        var missingIds = new List<int>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var dbIds = await _accountContext.ContactEntity.AsNoTracking().Select(c => c.ContactId).ToListAsync();
            missingIds = contactIds.Except(dbIds).ToList();
        });

        return missingIds;
    }

    private async Task<bool> CheckContactHasAccounts(int delegatorId, IEnumerable<int> accountIds)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var accountIdsExists = await _accountContext.RoleEntity
                .Where(r => r.ContactId == delegatorId)
                .Select(r => r.AccountId)
                .ToListAsync();

            return !accountIds.Any(id => !accountIdsExists.Contains(id));
        });
    }

    private async Task<IEnumerable<AccountEntity>> GetAccounts(IEnumerable<int> accountIds)
    {
        return await _accountContext.AccountEntity.Where(a => accountIds.Contains(a.AccountId)).ToListAsync();
    }

    private async Task<IEnumerable<int>> CheckExistingAccountsAsync(IEnumerable<int> accountIds)
    {
        var missingIds = new List<int>();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var dbIds = await _accountContext.AccountEntity.AsNoTracking().Select(a => a.AccountId).ToListAsync();
            missingIds = accountIds.Except(dbIds).ToList();
        });

        return missingIds;
    }

    private async Task<DelegationEntity> GetDelegationAsync(int delegationId)
    {
        DelegationEntity? tDelegation = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.DelegationEntity
                .Include(d => d.Account)
                .FirstOrDefaultAsync(d => d.DelegationId == delegationId);
        });

        if (tDelegation == null)
        {
            throw new NotFoundException(Errors.NotFoundDelegationCode, string.Format(Errors.NotFoundDelegationMessage, delegationId));
        }

        return tDelegation;
    }

    private async Task<IEnumerable<RoleEntity>> GetRolesForDelegationAsync(DelegationEntity delegationEntity)
    {
        var roles = Enumerable.Empty<RoleEntity>();
        var existingRoles = new List<RoleEntity>();
        var existingDelegations = Enumerable.Empty<DelegationEntity>();
        var accountIds = delegationEntity.Account.Select(a => a.AccountId);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            existingDelegations = await _accountContext.DelegationEntity
                .Include(d => d.Account)
                .Where(d => d.DelegationId != delegationEntity.DelegationId && d.Status.Equals(DelegationStatus.Enabled.ToString())
                    && d.DelegateeId == delegationEntity.DelegateeId && d.Account.Any(a => accountIds.Contains(a.AccountId)))
                .ToListAsync();

            roles = await _accountContext.RoleEntity
                .Where(r => accountIds.Contains(r.AccountId) && r.ContactId == delegationEntity.DelegateeId
                    && r.IsDelegation == true)
                .ToListAsync();
        });

        foreach (var delegation in existingDelegations)
        {
            foreach (var account in delegation.Account)
            {
                existingRoles.Add(new RoleEntity { ContactId = delegation.DelegateeId, AccountId = account.AccountId });
            }
        }

        return roles.Except(existingRoles, new RoleComparer());
    }

    private async Task<IEnumerable<RoleEntity>> RemoveDuplicateRoles(IEnumerable<RoleEntity> roles)
    {
        var accountIds = roles.Select(r => r.AccountId).Distinct();
        var contactIds = roles.Select(r => r.ContactId).Distinct();

        var existingRoles = await _accountContext
            .RoleEntity
            .AsNoTracking()
            .Where(r => accountIds.Contains(r.AccountId) && contactIds.Contains(r.ContactId))
            .ToListAsync();

        var rolesToCreate = roles.Except(existingRoles, new RoleComparer());
        return rolesToCreate;
    }

    public async Task<bool> IsClient(IEnumerable<int> contactIds)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.ContactEntity.AnyAsync(c => contactIds.Contains(c.ContactId) && c.Type == ContactType.Customer.ToString());
        });
    }
}
