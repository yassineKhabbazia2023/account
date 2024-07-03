// <copyright file="RoleRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
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
using Pulse.Account.Infrastructure.Mappers;

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
                throw new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage);
            }

            var result = await _accountContext.RoleEntity
                .AsNoTracking()
                .Include(x => x.Contact)
                .Where(x => x.AccountId == accountId && x.IsSignatory!.Value)
                .ToListAsync();

            return result.MapToContacts();
        });
    }

    public async Task<Role> GetContactRoleAsync(int accountId, int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var role = await _accountContext.RoleEntity
                .FirstOrDefaultAsync(r => r.AccountId == accountId && r.ContactId == contactId);

            return role!.MapToRole();
        });
    }

    public async Task<IEnumerable<Role>> CreateRoleAsync(CreateRoleRequest role)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!await _accountContext.AccountEntity.Include(a => a.DeploymentEntity)
                .AnyAsync(x => x.AccountId == role.AccountId && x.DeploymentEntity.First().Status != (int)DeploymentStatus.Revoked))
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, role.AccountId));
            }

            if (!await _accountContext.ContactEntity.AnyAsync(x => x.ContactId == role.ContactId && x.Status != ContactStatus.Removed.ToString()))
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, role.ContactId));
            }

            if (await GetContactRoleAsync(role.AccountId, role.ContactId) != null)
            {
                throw new BadRequestException(Errors.BadRequestExistingRoleCode, string.Format(Errors.BadRequestExistingRoleMessage, role.ContactId, role.AccountId));
            }

            var roles = new List<Role> { role.MapCreateRoleRequestToRole() };
            var roleEntities = roles.MapRolesToRolesDb();

            if (roleEntities.Any())
            {
                await _accountContext.RoleEntity.AddRangeAsync(roleEntities);
            }

            await _accountContext.SaveChangesAsync();

            return roles;
        });
    }

    public async Task<Role> UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var roles = from r in _accountContext.RoleEntity
                        where r.AccountId.Equals(accountId) && r.ContactId.Equals(contactId)
                        select r;

            var role = await roles.FirstOrDefaultAsync();
            if (role == null)
            {
                throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
            }

            if (role.IsSignatory != isSignatory)
            {
                role.IsSignatory = isSignatory;
                _accountContext.RoleEntity.Update(role);
                await _accountContext.SaveChangesAsync();
            }

            return role!.MapToRole();
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
}
