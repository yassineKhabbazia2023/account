// <copyright file="AccountService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Paging<Models.Account>> GetAccountsAsync(string? search, int pageNumber, int pageSize, int contactId, int? deploymentStatus)
        {
            pageNumber = Pagination.GetValidPageNumber(pageNumber);
            pageSize = Pagination.GetValidPageSize(pageSize);
            deploymentStatus = GetValidDeploymentStatus(deploymentStatus);
            return await _accountRepository.GetAccountsAsync(search, pageNumber, pageSize, contactId, deploymentStatus);
        }

        public async Task<AccountDetail?> GetAccountAsync(int accountId)
        {
            return await _accountRepository.GetAccountAsync(accountId);
        }

        public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
        {
            return await _accountRepository.GetAccountDetailAsync(accountId);
        }

        public async Task UpdateAccountAsync(int accountId, AccountDetail accountDetail)
        {
            await _accountRepository.UpdateAccountAsync(accountId, accountDetail);
        }

        public async Task<IEnumerable<Contact>> GetContactsAccountAsync(int accountId, ContactType? type)
        {
            return await _accountRepository.GetContactsAccountAsync(accountId, type);
        }

        public async Task<Paging<Contact>> GetContactsAccountByAdminAsync(string? search, int contactId, int pageNumber, int pageSize)
        {
            pageNumber = Pagination.GetValidPageNumber(pageNumber);
            pageSize = Pagination.GetValidPageSize(pageSize);
            return await _accountRepository.GetContactsAccountByAdminAsync(search, contactId, pageNumber, pageSize);
        }

        private static int? GetValidDeploymentStatus(int? deploymentStatus)
        {
            return deploymentStatus == null || System.Enum.IsDefined(typeof(DeploymentStatus), deploymentStatus)
                ? deploymentStatus
                : throw new NotFoundException(Errors.BadRequestDeploymentStatusCode, string.Format(Errors.BadRequestDeploymentStatusMessage, deploymentStatus));
        }
    }
}
