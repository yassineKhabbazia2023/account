// <copyright file="ContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Interfaces;

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

        public async Task<ContactEntity> GetContactAsync(int contactId)
        {
            var contact = await _accountContext.ContactEntity
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ContactId == contactId);

            if (contact == null)
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            return contact;
        }
    }
}
