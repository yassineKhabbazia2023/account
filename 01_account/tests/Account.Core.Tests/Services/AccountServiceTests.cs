// <copyright file="AccountServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
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
        private readonly Mock<IAccountRepository> _accountRepository;
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
        public async Task GetAccountsAsync_NotEmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(accountList);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountsAsync(search: string.Empty, contactId: 123, pageNumber: 1, pageSize: 4);

            // Assert
            Assert.Equal(accountList, accounts);
        }

        [Fact]
        public async Task GetAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(accountList);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountsAsync(search: string.Empty, contactId: 123, pageNumber: 0, pageSize: 0);

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
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<AccountDetail>(), It.IsAny<int>())).ReturnsAsync(accountDetail);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.UpdateAccountAsync(id: 1, accountDetail);

            // Assert
            Assert.Equal(accountDetail, accounts);
        }

        [Fact]
        public async Task GetContactsAccountAsync_WhenNotEmptyAccountId_ShouldReturnsContacts()
        {
            // Arrange
            var accountId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<List<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountAsync(It.IsAny<int>()))
                .Callback<int>(id => id.Should().Be(accountId))
                .ReturnsAsync(expected);

            var accountService = new AccountService(accountRepository.Object);

            // Act
            var result = await accountService.GetContactsAccountAsync(accountId);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
