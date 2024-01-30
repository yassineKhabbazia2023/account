// <copyright file="AccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Azure;
using Kpmg.Account.Core.Interfaces;
using Kpmg.Account.Core.Models;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
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

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit, int contactId)
        {
            try
            {
                return await this._retryPolicy.ExecuteAsync(async () =>
                {
                    var entities = from account in this._accountContext.TAccount
                                      select account;

                    await entities.ForEachAsync(entity =>
                    {
                        var roles = from role in this._accountContext.TRoles
                                    join contact in this._accountContext.TContact
                                    on role.ContactId equals contact.ContactId
                                    where role.AccountId == entity.AccountId
                                    select new { role, contact };

                        var deployments = from deployment in this._accountContext.TDeploymentPlanning
                                          where deployment.AccountId == entity.AccountId
                                          select deployment;

                        foreach (var role in roles)
                        {
                            role.role.Contact = role.contact;
                        }

                        entity.TRoles = roles.Select(role => role.role).ToList();
                        entity.TDeploymentPlanning = deployments.ToList();
                    });

                    if (!search.IsNullOrEmpty())
                    {
                        entities = from n in entities
                                      where n.LegalName.Contains(search)
                                        || n.SourceAccountNumber.Contains(search)
                                      select n;
                    }

                    var count = await entities.CountAsync();

                    entities = entities.Skip((page - 1) * limit);
                    entities = entities.Take(limit);

                    var pageinateResult = new Paging<AccountModel>()
                    {
                        Items = entities.Select(entity => entity.TAccountToAccountModel(contactId)),
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
