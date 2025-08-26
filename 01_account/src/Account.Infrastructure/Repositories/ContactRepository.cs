// <copyright file="ContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AccountContext _accountContext;

        public ContactRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;
            _accountContext.HandleEFCoreFailure();
        }

        public async Task<ContactEntity> GetContactAsync(int contactId, bool? searchDeleted = false)
        {

            var query = _accountContext.ContactEntity
                .AsNoTracking();
            if (searchDeleted == true)
            {
                query = query.IgnoreQueryFilters();
            }

            var contact = await query.FirstOrDefaultAsync(c => c.ContactId == contactId);
            if (contact == null)
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            return contact;
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
}
