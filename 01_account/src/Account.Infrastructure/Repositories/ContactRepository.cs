// <copyright file="ContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly AccountContext _accountContext;

    public ContactRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();
    }

    public async Task<Contact> GetContactByIdAsync(int contactId)
    {
        var query = _accountContext.ContactEntity.AsNoTracking();

        var contact = await query.FirstOrDefaultAsync(c => c.ContactId == contactId);

        if (contact == null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
        }

        return contact.MapToContact(null) !;
    }
}
