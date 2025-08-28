// <copyright file="IContactEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IContactEventRepository
{
    Task CreateContactAsync(ContactEntity contactEntity);

    Task UpdateContactAsync(ContactEntity contactEntity);

    Task RemoveContactAsync(int contactId);

    Task<ContactEntity?> GetContactById(int contactId);

    Task<bool> DoesContactExistAsync(int contactId);

    Task<ContactEntity> GetContactAsync(int contactId, bool? searchDeleted = false);

    Task<ContactEntity> GetContactByEmailAsync(string email);
}
