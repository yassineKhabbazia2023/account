// <copyright file="ContactEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactEventRepository : IContactEventRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ContactEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task CreateContactAsync(ContactEntity contactEntity)
    {
        await _accountContext.ContactEntity.AddAsync(contactEntity);
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task RemoveContactAsync(int contactId)
    {
        var existingContact = await _accountContext.ContactEntity.SingleAsync(x => x.ContactId == contactId);
        existingContact.Status = ContactStatus.Removed.ToString();
        existingContact.IsActive = false;
        existingContact.LastUpdateDate = DateTime.UtcNow;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task UpdateContactAsync(ContactEntity contactEntity)
    {
        var existingContact = await _accountContext.ContactEntity.SingleAsync(x => x.ContactId == contactEntity.ContactId);
        contactEntity.ToContactEntity(existingContact);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task<ContactEntity?> GetContactById(int contactId)
    {
        return await _accountContext.ContactEntity
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ContactId == contactId);
    }

    public async Task<bool> DoesContactExistAsync(int contactId)
    {
        return await _accountContext.ContactEntity.AnyAsync(c => c.ContactId == contactId);
    }

    public async Task<ContactEntity> GetContactAsync(int contactId, bool? searchDeleted = false)
    {
        var query = _accountContext.ContactEntity.AsNoTracking();

        if (searchDeleted == true)
        {
            query = query.IgnoreQueryFilters();
        }

        var contact = await query.FirstOrDefaultAsync(c => c.ContactId == contactId);

        return contact == null
            ? throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId))
            : contact;
    }

    public async Task<ContactEntity> GetContactByEmailAsync(string email)
    {
        var query = _accountContext.ContactEntity
            .AsNoTracking();

        var contact = await query.FirstOrDefaultAsync(c => c.Email == email);
        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, email));
        }

        return contact;
    }
}
