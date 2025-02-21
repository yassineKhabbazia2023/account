// <copyright file="RegistryRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class RegistryRoleEventRepository : IRegistryRoleEventRepository
{
    private readonly AccountContext _context;

    public RegistryRoleEventRepository(AccountContext context)
    {
        _context = context;
        _context.HandleEFCoreFailure();
    }

    public async Task<CreateRoleRequest> CreateRoleAsync(RegistryRoleCreatedEventData eventData)
    {
        (var accountId, var contactId) = await GetAccountIdContactIdAsync(eventData.AccountId, eventData.ContactId);

        var role = eventData.ToRoleEntity(accountId, contactId);

        _context.RoleEntity.Add(role);
        await _context.SaveChangesAsync();
        return role.ToCreateRoleRequest();
    }

    public async Task<(int, int)> RemoveRoleAsync(RegistryRoleRemovedEventData eventData)
    {
        (var accountId, var contactId) = await GetAccountIdContactIdAsync(eventData.AccountId, eventData.ContactId);

        var roleToRemove = _context.RoleEntity.FirstOrDefault(x => x.ContactId == contactId && x.AccountId == accountId);
        if (roleToRemove == null)
        {
            return (0, 0);
        }

        _context.RoleEntity.Remove(roleToRemove);
        await _context.SaveChangesAsync();
        return (accountId, contactId);
    }

    public async Task<(int, int)> GetAccountIdContactIdAsync(Guid accountId, Guid contactId)
    {
        var account = await _context.AccountEntity.FirstOrDefaultAsync(a => accountId == a.AccountGlobalUniqueId);
        var contact = await _context.ContactEntity.FirstOrDefaultAsync(c => contactId == c.ContactGlobalUniqueId);

        if (account == null || contact == null)
        {
            throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
        }

        return (account.AccountId, contact.ContactId);
    }
}
