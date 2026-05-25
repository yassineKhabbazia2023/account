// <copyright file="IAccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<AccountDetail> CreateAccountAsync(string currentUser, CreateAccountRequest request);

        Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination);

        Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination pagination, SearchAccountCriteria criteria);

        Task<Paging<AccountSearchResult>> SearchAccountsAsync(string? keyword, Pagination pagination);

        Task<Models.Account?> GetAccountSummaryAsync(int contactId, int accountId);

        Task<AccountDetail> GetAccountAsync(int accountId);

        Task<AccountDetail?> GetAccountDetailAsync(int accountId);

        Task<AccountDetail> UpdateAccountAsync(int accountId, AccountDetail accountDetail);

        Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination pagination);

        Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination pagination);

        Task<AccountDetail> GetAccountProspectIncludedAsync(int accountId);
    }
}
