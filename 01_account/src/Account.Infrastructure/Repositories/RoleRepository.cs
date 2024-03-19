// <copyright file="RoleRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Exceptions;
using Kpmg.ExceptionMiddleware.AdvancedException;

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
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
    }

    public async Task<Paging<Core.Models.Account>> GetContactRolesAsync(int contactId, int pageNumber, int pageSize)
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

            query = query.Skip((pageNumber - 1) * pageSize);
            query = query.Take(pageSize);

            var totalPages = Pagination.GetTotalPages(totalRows, pageSize);
            var entities = await query.ToListAsync();

            return MapAccountDbToAccountModel.MapToPaginAccounts(
                  entities,
                  contactId,
                  pageNumber,
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

    public async Task CreateRoleAsync(CreateRoleRequest role)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!_accountContext.AccountEntity.Any(x => x.AccountId == role.AccountId))
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, role.AccountId));
            }

            if (!_accountContext.ContactEntity.Any(x => x.ContactId == role.ContactId))
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, role.ContactId));
            }

            _accountContext.RoleEntity.Add(role.MapRoleToRoleDb());
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task UpdateRoleSignatoryAsync(int accountId, int contactId, bool isSignatory)
    {
        await _retryPolicy.ExecuteAsync(async () =>
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
        });
    }

    public async Task DeleteRoleAsync(int accountId, int contactId)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Theory an account will have at least 1 signataire
            // Verfiy if this account has another signataire beside this contact
            var signataire = _accountContext.RoleEntity
                            .Where(r => r.AccountId == accountId
                                && r.ContactId != contactId
                                && r.IsSignatory == true)
                            .FirstOrDefault();

            if (signataire == null) // this contact is the only signataire of this account
            {
                throw new BadRequestException(Errors.CannotDeleteSignatoryCode, Errors.CannotDeleteSignatoryMessage);
            }

            var role = _accountContext.RoleEntity
                        .Where(r => r.AccountId == accountId && r.ContactId == contactId)
                        .FirstOrDefault();
            if (role == null)
            {
                throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
            }

            _accountContext.Remove(role);
            await _accountContext.SaveChangesAsync();

        });
    }
}
