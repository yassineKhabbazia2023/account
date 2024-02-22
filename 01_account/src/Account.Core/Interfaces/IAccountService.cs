// <copyright file="IAccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountService
    {
        public Task<Paging<Models.Account>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId);

        public Task<AccountDetail?> GetAccountDetailAsync(int id);

        public Task<AccountDetail?> UpdateAccountAsync(int id, AccountDetail accountDetail);

        Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId);
    }
}
