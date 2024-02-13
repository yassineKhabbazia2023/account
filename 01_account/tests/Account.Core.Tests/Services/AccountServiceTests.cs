// <copyright file="AccountServiceTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Core.Tests.Services
{
    public class AccountServiceTests
    {
        private Mock<IAccountRepository> _accountRepository;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AccountServiceTests()
        {
            _accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
        }

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            _accountRepository.Setup(repository => repository.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountsAsync(search: string.Empty, contactId: 123, page: 1, limit: 4);

            // Assert
            Assert.Equal(accountList, accounts);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountMocked, _jsonOptions) ?? new AccountDetail();
            _accountRepository.Setup(repository => repository.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountDetail);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountDetailAsync(id: 1);

            // Assert
            Assert.Equal(accountDetail, accounts);
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountMocked, _jsonOptions) ?? new AccountDetail();
            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<AccountDetail>(), It.IsAny<int>())).ReturnsAsync(accountDetail);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.UpdateAccountAsync(id: 1, accountDetail);

            // Assert
            Assert.Equal(accountDetail, accounts);
        }

        [Fact]
        public async Task Should_GetStatistics_Nominal()
        {
            var expected = new Statistics
            {
                AccountConnected = 10,
                AccountToDeploy = 3,
                AccountInProgress = 5
            };

            _accountRepository.Setup(r => r.GetStatisticsAsync(It.IsAny<int>())).ReturnsAsync(expected);
            var accountService = new AccountService(_accountRepository.Object);

            var result = await accountService.GetStatisticsAsync(It.IsAny<int>());

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }
    }
}
