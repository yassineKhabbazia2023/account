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
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Models.Constants;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountContext _accountContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public AccountRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;

            _retryPolicy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 1,
                        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Constants.RETRYTIMESPAN));
        }

        public async Task<Paging<AccountModel>> GetAccountsAsync(string? search, int page, int limit, int contactId)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    IQueryable<TAccount> entities = GetAccountQueryByContactId(contactId);

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
                throw new TechnicalException(Errors.InternalTechnicalError, ex);
            }
        }

        public async Task<AccountDetail> GetAccountDetailAsync(int accountId)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    IQueryable<TAccount> entities = _accountContext.TAccount
                           .AsNoTracking()
                           .Include(x => x.TRoles)
                           .ThenInclude(r => r.Contact)
                           .Include(a => a.TAddress)
                           .Include(x => x.TDeploymentPlanning)
                           .Include(x => x.Hub)
                           .Include(x => x.TPhone)
                           .Where(a => a.AccountId == accountId);

                    var entity = entities.FirstOrDefault();
                    if(entity == null)
                    {
                        throw new NotFoundException(HttpStatusCode.NotFound.ToString(), Errors.NotFoundError);
                    }

                    var accountDetail = entity.TAccountToAccountDetail();

                    return accountDetail;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new TechnicalException(Errors.InternalTechnicalError, ex);
            }
        }

        private IQueryable<TAccount> GetAccountQueryByContactId(int contactId)
        {
            return _accountContext.TAccount
                            .AsNoTracking()
                            .Include(x => x.TRoles)
                            .ThenInclude(r => r.Contact)
                            .Include(a => a.TAddress)
                            .Include(x => x.TDeploymentPlanning)
                            .Where(a => a.TRoles.Any(r => r.ContactId == contactId));
        }
    }
}
