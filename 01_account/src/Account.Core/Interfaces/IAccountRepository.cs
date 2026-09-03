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

        /// <summary>
        /// Clears the dematerialization email address of an account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns><c>true</c> when the account exists; otherwise, <c>false</c>.</returns>
        Task<bool> ResetDematEmailAsync(int accountId);

        Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination, bool sortByLastActivity = true);

        Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination pagination, SearchAccountCriteria criteria);

        Task<Paging<AccountSearchResult>> SearchAccountsAsync(string? keyword, Pagination pagination);

        Task<Models.Account?> GetAccountSummaryAsync(int contactId, int accountId);

        Task<AccountDetail> GetAccountAsync(int accountId);

        Task<AccountDetail?> GetAccountDetailAsync(int accountId);

        Task<AccountDetail> UpdateAccountAsync(int accountId, AccountDetail accountDetail, bool includeProspects = false);

        Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination pagination, bool includeProspects = false);

        /// <summary>
        /// Gets account collaborators with contact widget role labels.
        /// </summary>
        /// <param name="accountId">Account identifier.</param>
        /// <returns>Collaborators having the AM or CLP role label on the account.</returns>
        Task<IEnumerable<Contact>> GetAccountContactWidgetContactsAsync(int accountId);

        /// <summary>
        /// Checks whether an existing contact has roles only on prospect accounts.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns><c>true</c> when the contact has at least one role and all linked accounts are prospects; otherwise, <c>false</c>.</returns>
        Task<bool> IsContactProspectOnlyAsync(int contactId);

        Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination pagination);

        Task<AccountDetail> GetAccountProspectIncludedAsync(int accountId);
    }
}
