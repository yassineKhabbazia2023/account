using System.Collections.Generic;
using System.Net;
using System.Text;
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
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);

            var accountController = new AccountController(accountService.Object);

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
            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountDetail);

            var accountController = new AccountController(accountService.Object);

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
            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>())).ReturnsAsync(accountDetail);

            var accountController = new AccountController(accountService.Object);

            // Act
            var accounts = await accountController.UpdateAccountAsync(accountId: 1, accountDetail);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountDetail, resultAccounts?.Value);
        }

        [Fact]
        public void Should_GetAccountFavoriteList_ReturnsOkResultAsync()
        {
            // Arrange
            var accountJson = new List<AccountFavorite>()
            {
                new AccountFavorite()
                {
                    AccountId = "93012CC8-77B9-4161-8DBD-61915D935E21",
                    LegalName = "JEAN LEVAGE",
                    Icon = "jeanlevage"
                }
            };
            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetAccountFavoritesAsync(It.IsAny<int>())).Returns(accountJson);

            var accountController = new AccountController(accountService.Object);

            // Act
            var accounts = accountController.GetAccountFavoritesAsync(contactId: 1);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountJson, resultAccounts?.Value);
        }
    }
}
