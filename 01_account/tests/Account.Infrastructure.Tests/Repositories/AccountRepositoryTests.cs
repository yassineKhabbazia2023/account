// <copyright file="AccountRepositoryTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using Kpmg.Account.Core.Models;
using Kpmg.Account.Infrastructure.Repositories;
using Kpmg.Account.Infrastructure.Tests.Configuration;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Newtonsoft.Json;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using AccountModel = Kpmg.Account.Core.Models.Account;

namespace Kpmg.Account.Infrastructure.Tests.Repositories
{
    public class AccountRepositoryTests
    {
        private readonly Fixture _fixture;

        public AccountRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            // Arrange
            var accountsModel = _fixture.Create<List<TAccount>>();
            var accountRepository = UnitTestUtils.InitAccountRepository(_fixture, accountsModel);
            var accountObject = accountsModel.Select(item => item.TAccountToAccountModel(123));
            Paging<AccountModel> accountPaging = new Paging<AccountModel>()
            {
                CurrentPage = 1,
                Items = accountObject,
                TotalItems = accountObject.Count(),
                TotalPage = 1
            };

            // Act
            var accounts = await accountRepository.GetAccountsAsync(search: accountObject.Select(a => a.LegalName).First(), contactId: 123, page: 1, limit: 4);

            // Assert
            var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
            var accountReceived = JsonConvert.SerializeObject(accounts.Items?.FirstOrDefault());
            Assert.Contains(accountReceived, accountExpect);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountsModel = _fixture.Create<List<TAccount>>();
            var accountFirst = accountsModel.FirstOrDefault();
            var accountDetail = accountFirst?.TAccountToAccountDetail();
            var accountRepository = UnitTestUtils.InitAccountRepository(_fixture, accountsModel);

            // Act
            var accounts = await accountRepository.GetAccountDetailAsync(accountFirst.AccountId);

            // Assert
            Assert.Equal(accountDetail?.AccountNumber, accounts.AccountNumber);
            Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
            Assert.Equal(accountDetail?.Legal?.LegalName, accounts.Legal?.LegalName);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsNotFoundResultAsync()
        {
            // Arrange
            var accountsModel = _fixture.Create<List<TAccount>>();
            var accountFirst = accountsModel.FirstOrDefault();
            var accountDetail = accountFirst?.TAccountToAccountDetail();
            var accountRepository = UnitTestUtils.InitAccountRepository(_fixture, accountsModel);

            // Act
            Task Accounts() => accountRepository.GetAccountDetailAsync(123);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(Accounts);
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync()
        {
            // Arrange
            var accountsModel = _fixture.Create<List<TAccount>>();
            var accountFirst = accountsModel.FirstOrDefault();
            var accountDetail = accountFirst?.TAccountToAccountDetail();
            if(accountDetail?.Accounting != null)
            {
                accountDetail.Accounting.TaxationSystem = "Impot sur le revenu";
            }

            var accountRepository = UnitTestUtils.InitAccountRepository(_fixture, accountsModel);

            // Act
            var accounts = await accountRepository.UpdateAccountAsync(accountDetail, accountDetail.AccountId);

            // Assert
            Assert.Equal(accountDetail?.AccountNumber, accounts.AccountNumber);
            Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
            Assert.Equal(accountDetail?.Accounting?.TaxationSystem, accounts.Accounting?.TaxationSystem);
        }
    }
}
