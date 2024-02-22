// <copyright file="AccountControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
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
using AutoFixture;

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
            var accounts = await accountController.GetAccountsAsync(search: string.Empty, contactId: 123, pageNumber: 1, pageSize: 4);
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
        public async Task GetContactsAccountAsync_Should_Returns_Contacts_Account()
        {
            // Arrange
            var accountId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<List<Contact>>();

            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetContactsAccountAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);
            var accountController = new AccountController(accountService.Object);

            // Act
            var result = await accountController.GetContactsAccountAsync(accountId);

            // Assert
            Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        }
    }
}
