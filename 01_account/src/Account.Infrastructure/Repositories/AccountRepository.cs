// <copyright file="AccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Data;
using System.Net;
using Azure;
using Kpmg.Account.Core.Interfaces;
using Kpmg.Account.Core.Models;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
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

                    entities.ToList().ForEach(entity =>
                    {
                        var roles = from role in this._accountContext.TRoles
                                    join contact in this._accountContext.TContact
                                    on role.ContactId equals contact.ContactId
                                    where role.AccountId == entity.AccountId
                                    select new { role, contact };

                        var deployments = from deployment in this._accountContext.TDeploymentPlanning
                                          where deployment.AccountId == entity.AccountId
                                          select deployment;

                        var addressList = from address in this._accountContext.TAddress
                                          where address.AccountId == entity.AccountId
                                          select address;

                        foreach (var roleItem in roles)
                        {
                            roleItem.role.Contact = roleItem.contact;
                        }

                        entity.TAddress = addressList.ToList();
                        entity.TRoles = roles.Select(role => role.role).ToList();
                        entity.TDeploymentPlanning = deployments.ToList();
                    });

                    entities = entities.Where(entity => entity.TRoles.Any(role => role.ContactId == contactId));
                    if (!search.IsNullOrEmpty())
                    {
                        entities = from n in entities
                                   where n.LegalName.Contains(search)
                                         || n.SourceAccountNumber.Contains(search)
                                         || n.TRoles.Any(role => role.Contact.FirstName.Contains(search)
                                                             || role.Contact.LastName.Contains(search)
                                                             || role.Contact.ContactEmail.Contains(search))
                                   select n;
                    }

                    var count = await entities.CountAsync();

                    entities = entities.Skip((page - 1) * limit);
                    entities = entities.Take(limit);

                    var totalPageCalcul = count != 0 ? count / (limit > count ? count : (float)limit) : 0;

                    var pageinateResult = new Paging<AccountModel>()
                    {
                        Items = entities.Select(entity => entity.TAccountToAccountModel(contactId)),
                        CurrentPage = page,
                        TotalItems = count,
                        TotalPage = (int)Math.Ceiling(totalPageCalcul)
                    };
                    return pageinateResult;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new TechnicalException(ExceptionsConstants.InternalTechnicalError, ex);
            }
        }

        public async Task<AccountDetail> GetAccountDetailAsync(Guid accountId)
        {
            try
            {
                return await this._retryPolicy.ExecuteAsync(async () =>
                {
                    var entities = from account in this._accountContext.TAccount
                                   where account.AccountGlobalUniqueId.Equals(accountId)
                                    select account;

                    var entity = entities.FirstOrDefault();
                    if(entity == null)
                    {
                        throw new NotFoundException(HttpStatusCode.NotFound.ToString(), ExceptionsConstants.NotFoundError);
                    }

                    var roles = from role in this._accountContext.TRoles
                                join contact in this._accountContext.TContact
                                on role.ContactId equals contact.ContactId
                                where role.AccountId == entity.AccountId
                                select new { role, contact };

                    var deployments = from deployment in this._accountContext.TDeploymentPlanning
                                        where deployment.AccountId == entity.AccountId
                                        select deployment;

                    var addressList = from address in this._accountContext.TAddress
                                        where address.AccountId == entity.AccountId
                                        select address;

                    var phoneList = from phones in this._accountContext.TPhone
                                      where phones.AccountId == entity.AccountId
                                      select phones;

                    foreach (var roleItem in roles)
                    {
                        roleItem.role.Contact = roleItem.contact;
                    }

                    entity.TAddress = addressList.ToList();
                    entity.TPhone = phoneList.ToList();
                    entity.TRoles = roles.Select(role => role.role).ToList();
                    entity.TDeploymentPlanning = deployments.ToList();

                    var accountDetail = entity.TAccountToAccountDetail();

                    return accountDetail;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new TechnicalException(ExceptionsConstants.InternalTechnicalError, ex);
            }
        }
    }
}
