// <copyright file="RegistryRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
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

    public async Task<CreateRoleRequest> CreateRoleAsync(RegistryRoleCreatedEventData eventData, int? accountId, int? contactId, bool? isCustomerRelation)
    {
        var role = eventData.ToRoleEntity(accountId, contactId, isCustomerRelation);

        _context.RoleEntity.Add(role);
        await _context.SaveChangesAsync();
        return role.ToCreateRoleRequest();
    }

    public async Task<CreateRoleRequest?> UpdateRoleContactFlagPortailFacturesAsync(int accountId, int contactId, bool? contactFlagPortailFactures)
    {
        var existingRole = await _context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == accountId && r.ContactId == contactId);
        if (existingRole != null)
        {
            existingRole.ContactFlagPortailFactures = contactFlagPortailFactures;
            _context.RoleEntity.Update(existingRole);
            await _context.SaveChangesAsync();
            return existingRole.ToCreateRoleRequest();
        }

        return null;
    }

    public async Task<bool> RemoveRoleAsync(int accountId, int contactId)
    {
        await CheckExistingAccountAndContactAsync(accountId, contactId);

        var roleToRemove = _context.RoleEntity.FirstOrDefault(x => x.ContactId == contactId && x.AccountId == accountId);

        if (roleToRemove == null)
        {
            return false;
        }

        _context.RoleEntity.Remove(roleToRemove);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task CheckExistingAccountAndContactAsync(int accountId, int contactId)
    {
        var account = await _context.AccountEntity.FirstOrDefaultAsync(a => accountId == a.AccountId);
        var contact = await _context.ContactEntity.FirstOrDefaultAsync(c => contactId == c.ContactId);

        if (account == null || contact == null)
        {
            throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, contactId, accountId));
        }
    }

    public async Task<int> GetAccountIdByGuidAsync(Guid accountId)
    {
        var account = await _context.AccountEntity.FirstOrDefaultAsync(a => accountId == a.AccountGlobalUniqueId);

        if (account == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId.ToString()));
        }

        return account.AccountId;
    }

    public async Task<ContactEntity> GetContactByGuidAsync(Guid contactId)
    {
        var contact = await _context.ContactEntity.FirstOrDefaultAsync(c => contactId == c.ContactGlobalUniqueId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId.ToString()));
        }

        return contact;
    }
}
