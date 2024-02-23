// <copyright file="RoleRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using System.Net;
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
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Core.Models.Exceptions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Account.Core.Requests;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            IQueryable<TAccount> query = _accountContext.TAccount
                                                        .AsNoTracking()
                                                        .Include(x => x.TRole)
                                                        .ThenInclude(r => r.Contact)
                                                        .Include(a => a.TAddress)
                                                        .Include(x => x.TDeploymentPlanning)
                                                        .Where(a => a.TRole.Any(r => r.ContactId == contactId))
                                                        .OrderBy(x => x.LegalName);

            var totalRows = await query.CountAsync();

            query = query.Skip((pageNumber - 1) * pageSize);
            query = query.Take(pageSize);

            var totalPages = PagesCalculator.GetTotalPages(totalRows, pageSize);
            var entities = await query.ToListAsync();

            return MapAccountDbToAccountModel.MapToPaginAccounts(
                  entities,
                  contactId,
                  pageNumber,
                  totalRows,
                  totalPages);

        }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Contact>> GetSignatoryAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var result = await _accountContext.TRole
                .AsNoTracking()
                .Include(x => x.Contact)
                .Where(x => x.AccountId == accountId && x.IsSignatory!.Value)
                .ToListAsync();

            return result.MapToContacts();
        }).ConfigureAwait(false);
    }

    public async Task CreateRoleAsync(CreateRole role)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            _accountContext.TRole.Add(role.MapRoleToRoleDb());
            await _accountContext.SaveChangesAsync();
        }).ConfigureAwait(false);
    }

    public async Task UpdateRoleAsync(int accountId, int contactId, bool isSignatory)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var existingRole = from role in _accountContext.TRole
                               where role.AccountId.Equals(accountId) && role.ContactId.Equals(contactId)
                               select role;

            var existingRoleItem = await existingRole.FirstOrDefaultAsync();
            if (existingRoleItem != null)
            {
                existingRoleItem.IsSignatory = isSignatory;
                _accountContext.TRole.Update(existingRoleItem);
                await _accountContext.SaveChangesAsync();
            }
        }).ConfigureAwait(false);
    }
}
