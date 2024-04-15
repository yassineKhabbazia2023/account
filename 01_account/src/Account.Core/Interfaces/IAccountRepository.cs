// <copyright file="IAccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria);

        Task<AccountDetail?> GetAccountAsync(int accountId);

        Task<AccountDetail?> GetAccountDetailAsync(int accountId);

        Task UpdateAccountAsync(int accountId, AccountDetail accountDetail);

        Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId, ContactType? type);

        Task<Paging<Contact>> GetContactsAccountByAdminAsync(string? search, int contactId, int pageNumber, int pageSize);
    }
}
