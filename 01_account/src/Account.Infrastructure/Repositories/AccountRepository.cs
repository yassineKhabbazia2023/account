// <copyright file="AccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Azure;
using Kpmg.Account.Core.Interfaces;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Models;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountContext _accountContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public AccountRepository(AccountContext accountContext)
        {
            this._accountContext = accountContext;

            this._retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRY_TIMESPAN));
        }

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit)
        {
            try
            {
                return await this._retryPolicy.ExecuteAsync(async () =>
                {
                    var accountList = from n in this._accountContext.Accounts
                                    select n;
                    if (!search.IsNullOrEmpty())
                    {
                        accountList = from n in accountList
                                      where n.LegalName.Contains(search) || n.AccountNumber.Contains(search)
                                    select n;
                    }

                    var count = await accountList.CountAsync();

                    accountList = accountList.Skip((page - 1) * limit);
                    accountList = accountList.Take(limit);

                    var pageinateResult = new Paging<AccountModel>()
                    {
                        Items = accountList,
                        CurrentPage = page,
                        TotalItems = count,
                        TotalPage = count / limit
                    };
                    return pageinateResult;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new TechnicalException(ExceptionsConstants.InternalTechnicalError, ex);
            }
        }
    }
}
