// <copyright file="AccountControllerTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Pulse.Account.API;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Account.Api.Tests.Controllers
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly Mock<IAccountService> _accountService;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AccountControllerTests()
        {
            _accountService = new Mock<IAccountService>(MockBehavior.Strict);
        }

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            _accountService.Setup(service => service.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);

            var accountController = new AccountController(_accountService.Object);

            // Act
            var accounts = await accountController.GetAccountsAsync(search: string.Empty, contactId: 123, page: 1, limit: 4);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountList, resultAccounts?.Value);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountMocked, _jsonOptions) ?? new AccountDetail();
            _accountService.Setup(service => service.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountDetail);

            var accountController = new AccountController(_accountService.Object);

            // Act
            var accounts = await accountController.GetAccountDetailAsync(1);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountDetail, resultAccounts?.Value);
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountDetailMocked.json");
            var accountDetail = JsonSerializer.Deserialize<AccountDetail>(accountMocked, _jsonOptions) ?? new AccountDetail();
            _accountService.Setup(service => service.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>())).ReturnsAsync(accountDetail);

            var accountController = new AccountController(_accountService.Object);

            // Act
            var accounts = await accountController.UpdateAccountAsync(accountId: 1, accountDetail);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountDetail, resultAccounts?.Value);
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

            _accountService.Setup(service => service.GetStatisticsAsync(It.IsAny<int>())).ReturnsAsync(expected);
            var accountController = new AccountController(_accountService.Object);

            var statistics = await accountController.GetStatistics(It.IsAny<int>());
            var result = statistics?.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(expected, result.Value);

        }
    }
}
