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
        Task<AccountDetail> CreateAccountAsync(int currentUserId, CreateAccountRequest request);

        public Task<Paging<Models.Account>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination? pagination, string? currentUserEmail = null);

        public Task<Paging<Models.Account>> GetAllAccountsAsync(string? accountNumber, Pagination? pagination, SearchAccountCriteria? criteria);

        public Task<Paging<AccountSearchResult>> SearchAccountsAsync(string? keyword, Pagination? pagination);

        public Task<Models.Account?> GetAccountSummaryAsync(int contactId, int accountId, string contactType);

        public Task<AccountDetail?> GetAccountAsync(int accountId);

        /// <summary>
        /// Gets the account detail enriched with the demat mail collect modal closure state for the requesting contact.
        /// </summary>
        /// <param name="accountId">Account identifier.</param>
        /// <param name="contactId">The requesting contact identifier (CurrentUser header), when known. When null (e.g. non-gateway callers), <see cref="AccountDetail.ModalClosedAt"/> is left unset.</param>
        /// <returns>Account detail with <see cref="AccountDetail.ModalClosedAt"/> set, or null if the account does not exist.</returns>
        public Task<AccountDetail?> GetAccountDetailAsync(int accountId, int? contactId);

        /// <summary>
        /// Closes the demat mail collect modal for the requesting contact on the given account.
        /// </summary>
        /// <param name="accountId">Account identifier.</param>
        /// <param name="contactId">The contact closing the modal (CurrentUser header).</param>
        /// <returns>A <see cref="Result"/> indicating success, or <see cref="ResultStatus.NotFound"/> when the account does not exist.</returns>
        public Task<Result> CloseDematModalAsync(int accountId, int contactId);

        public Task UpdateAccountAsync(int accountId, AccountDetail accountDetail, string? currentUserEmail = null);

        public Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination? pagination, string? currentUserEmail = null);

        /// <summary>
        /// Gets account collaborators with contact widget role labels.
        /// </summary>
        /// <param name="accountId">Account identifier.</param>
        /// <returns>Collaborators having the AM or CLP role label on the account.</returns>
        public Task<IEnumerable<Contact>> GetAccountContactWidgetContactsAsync(int accountId);

        /// <summary>
        /// Checks whether an existing contact has roles only on prospect accounts.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns><c>true</c> when the contact has at least one role and all linked accounts are prospects; otherwise, <c>false</c>.</returns>
        Task<bool> IsContactProspectOnlyAsync(int contactId);

        Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination? pagination);
    }
}
