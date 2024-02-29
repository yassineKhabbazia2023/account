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
using Microsoft.AspNetCore.JsonPatch;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Pulse.Account.Core.Exceptions;

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
            _accountService.Setup(service => service.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(accountDetail);
            _accountService.Setup(service => service.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>()))
                .Callback<int, AccountDetail>((id, account) =>
                {
                    id.Should().Be(accountDetail.AccountId);
                })
                .Returns(Task.CompletedTask);

            var jsonPatch = new JsonPatchDocument<AccountDetail>();
            jsonPatch.Replace(a => a.Hub, accountDetail.Hub);
            jsonPatch.Replace(a => a.Accounting, accountDetail.Accounting);
            jsonPatch.Replace(a => a.Legal!.StaffSizeRange, accountDetail.Legal?.StaffSizeRange);
            var accountController = new AccountController(_accountService.Object);

            // Act
            var result = await accountController.UpdateAccountAsync(accountId: accountDetail.AccountId, jsonPatch) as OkResult;

            // Assert
            Assert.Equal(200, result!.StatusCode);
        }

        [Fact]
        public async Task UpdateAccountAsync_WithAccountPatchNull_ShouldThrowBadRequestException()
        {
            var controller = new AccountController(_accountService.Object);

            var result = await Assert.ThrowsAsync<BadRequestException>(async () => await controller.UpdateAccountAsync(It.IsAny<int>(), null!));

            Assert.Equal(Errors.BadRequestAccountPatchCode, result.Code);
            Assert.Equal(Errors.BadRequestAccountPatchMessage, result.Message);
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
