// <copyright file="IAccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Paging<Models.Account>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId);

        Task<AccountDetail?> GetAccountDetailAsync(int accountId);

        Task<AccountDetail?> UpdateAccountAsync(AccountDetail accountDetail, int accountId);

        Task<Statistics> GetStatisticsAsync(int contactId);

        Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId);
    }
}
