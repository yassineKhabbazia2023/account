// <copyright file="IAccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountService
    {
        public Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination? pagination);

        public Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination? pagination, SearchAccountCriteria? criteria);

        public Task<Models.Account?> GetAccountSummaryAsync(int contactId, int accountId);

        public Task<AccountDetail?> GetAccountAsync(int accountId);

        public Task<AccountDetail?> GetAccountDetailAsync(int accountId);

        public Task UpdateAccountAsync(int accountId, AccountDetail accountDetail);

        public Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination? pagination);

        Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination? pagination);
    }
}
