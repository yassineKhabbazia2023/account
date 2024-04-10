// <copyright file="AccountControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pulse.Account.API;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Account.Api.Tests.Controllers
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _dbContextOptions;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private AccountController _accountController;
        private AccountContext _context;

        public AccountControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = InitContext();
            var accountRepository = new AccountRepository(_context);
            var accountService = new AccountService(accountRepository);
            _accountController = new AccountController(accountService);
        }

        ~AccountControllerTests()
        {
            _context.Dispose();
        }

        private AccountContext InitContext()
        {
            var context = new AccountContext(_dbContextOptions);
            var accountsModel = _fixture.Create<List<AccountEntity>>();
            var contactsModel = _fixture.Create<List<ContactEntity>>();

            context.AccountEntity.AddRange(accountsModel);
            context.ContactEntity.AddRange(contactsModel);
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            // Arrange
            var account = _context.AccountEntity.First();
            var contact = _context.ContactEntity.First();

            // Act
            var accounts = await _accountController.GetAccountsAsync(search: string.Empty, contactId: contact.ContactId, pageNumber: 1, pageSize: 4);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(account.AccountId, resultAccounts!.Value.As<Paging<AccountModel>>().Items!.First().AccountId);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _context.AccountEntity.First();

            // Act
            var account = await _accountController.GetAccountDetailAsync(accountMocked.AccountId);
            var resultAccounts = account?.Result as OkObjectResult;

            // Assert
            Assert.Equivalent(MapAccountDbToAccountModel.MapToAccountDetail(accountMocked), resultAccounts!.Value.As<AccountDetail>());
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync()
        {
            // Arrange
            var accountId = _context.AccountEntity.First().AccountId;
            var newHub = _fixture.Create<HubEntity>();
            _context.HubEntity.Add(newHub);
            _context.SaveChanges();

            var jsonPatch = new JsonPatchDocument<AccountDetail>();
            jsonPatch.Replace(a => a.Hub, new Hub() { HubId = newHub.HubId, HubName = newHub.HubName });

            // Act
            var result = await _accountController.UpdateAccountAsync(accountId, jsonPatch) as OkResult;
            var accountDetail = await _accountController.GetAccountDetailAsync(accountId);
            var accountDetailResult = accountDetail.Result as OkObjectResult;

            // Assert
            Assert.Equal(200, result!.StatusCode);
            Assert.Equal(newHub.HubId, accountDetailResult!.Value.As<AccountDetail>().Hub!.HubId);
            Assert.Equal(newHub.HubName, accountDetailResult!.Value.As<AccountDetail>().Hub!.HubName);
        }

        [Fact]
        public async Task UpdateAccountAsync_WithAccountPatchNull_ShouldThrowBadRequestException()
        {
            using (var context = InitContext())
            {
                // Arrange

                // Act
                var result = await Assert.ThrowsAsync<BadRequestException>(async () => await _accountController.UpdateAccountAsync(It.IsAny<int>(), null!));

                // Assert
                Assert.Equal(Errors.BadRequestAccountPatchCode, result.Code);
                Assert.Equal(Errors.BadRequestAccountPatchMessage, result.Message);
            }
        }

        [Fact]
        public async Task GetContactsAccountAsync_Should_Returns_Contacts_Account()
        {
            // Arrange
            var accountId = 6000;
            var expected = _fixture.Create<List<Contact>>();

            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<ContactType?>()))
                .ReturnsAsync(expected);
            var accountController = new AccountController(accountService.Object);

            // Act
            var result = await accountController.GetContactsAccountAsync(accountId, null!);

            // Assert
            Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        }

        [Fact]
        public async Task GetContactsAccountByAdminAsync_Should_Returns_Contacts_Account()
        {
            // Arrange
            var contactId = 6000;
            var expected = _fixture.Create<Paging<Contact>>();

            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetContactsAccountByAdminAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expected);
            var accountController = new AccountController(accountService.Object);

            // Act
            var result = await accountController.GetContactsAccountByAdminAsync(string.Empty, contactId, 1, 999);

            // Assert
            Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        }

        [Fact]
        public async Task GetContactsAccountByAdminAsync_Should_Throw_NotFoundException()
        {
            // Arrange
            var contactId = 6000;
            var accountService = new Mock<IAccountService>(MockBehavior.Strict);
            accountService.Setup(service => service.GetContactsAccountByAdminAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new NotFoundException(Errors.NotFoundRoleContactCode, Errors.NotFoundRoleContactMessage));
            var accountController = new AccountController(accountService.Object);

            // Act
            var result = async () => await accountController.GetContactsAccountByAdminAsync(string.Empty, contactId, 1, 999);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundRoleContactCode, exception.Code);
            Assert.Equal(Errors.NotFoundRoleContactMessage, exception.Message);
        }
    }
}
