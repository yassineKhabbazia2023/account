// <copyright file="AccountRepositoryTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using Pulse.Account.Infrastructure.Tests.Configuration;
using Newtonsoft.Json;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Tests.Repositories
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
            var accounts = await accountRepository.GetAccountsAsync(search: string.Empty, contactId: 123, page: 1, limit: 4);

            // Assert
            var accountExpect = JsonConvert.SerializeObject(accountPaging);
            var accountReceived = JsonConvert.SerializeObject(accounts);
            Assert.Equal(accountExpect, accountReceived);
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
            var accounts = await accountRepository.GetAccountDetailAsync(accountFirst!.AccountId);

            // Assert
            Assert.Equal(accountDetail?.AccountNumber, accounts.AccountNumber);
            Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
            Assert.Equal(accountDetail?.Legal?.LegalName, accounts.Legal?.LegalName);
        }
    }
}
