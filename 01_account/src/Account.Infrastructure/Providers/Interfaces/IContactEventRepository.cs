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

    Task<IEnumerable<int>> UpdateAccountStatusByContactAsync(IEnumerable<int> accountIds, int deploymentStatus);

    List<AccountEntity> GetAccountBySignatory(int contactId);

    ContactEntity? GetContactById(int contactId);
}
