// <copyright file="AccountServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Core.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepository;

        private readonly Fixture _fixture;

        public AccountServiceTests()
        {
            _accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetAccountsAsync_NotEmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountsAsync(search: string.Empty, contactId: 123, pageNumber: 1, pageSize: 4);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountsAsync(search: string.Empty, contactId: 123, pageNumber: 0, pageSize: 0);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountDetailAsync(accountId: 1);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsAccounttAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            var accounts = await accountService.GetAccountAsync(accountId: 1);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>()))
                .Returns(Task.CompletedTask);

            var accountService = new AccountService(_accountRepository.Object);

            // Act
            await accountService.UpdateAccountAsync(accountId: 1, accountMocked);

            // Assert
            _accountRepository.Verify(repository => repository.UpdateAccountAsync(1, accountMocked));
        }

        [Fact]
        public async Task GetContactsAccountAsync_WhenNotEmptyAccountId_ShouldReturnsContacts()
        {
            // Arrange
            var accountId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<List<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<ContactType?>()))
                .Callback<int, ContactType?>((id, type) => id.Should().Be(accountId))
                .ReturnsAsync(expected);

            var accountService = new AccountService(accountRepository.Object);

            // Act
            var result = await accountService.GetContactsAccountAsync(accountId, null!);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
