// <copyright file="IContactRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Interfaces
{
    public interface IContactRepository
    {
        Task<ContactEntity> GetContactAsync(int contactId, bool? searchDeleted = false);
    }
}
