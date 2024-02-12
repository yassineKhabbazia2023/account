// <copyright file="IAccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Paging<Models.Account>> GetAccountsAsync(string? search, int page, int limit, int contactId);

        Task<AccountDetail?> GetAccountDetailAsync(int accountId);

        Task<AccountDetail?> UpdateAccountAsync(AccountDetail accountDetail, int accountId);
    }
}
