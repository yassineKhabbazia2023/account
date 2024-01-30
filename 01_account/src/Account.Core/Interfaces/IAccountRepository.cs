// <copyright file="IAccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models.Utils;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit, int contactId);
    }
}
