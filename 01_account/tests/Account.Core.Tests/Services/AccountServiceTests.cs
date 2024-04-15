// <copyright file="AccountServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Moq;
using Pulse.Account.Core.Exceptions;
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
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                PageNumber = 1,
                PageSize = 4,
                Search = string.Empty,
                ContactId = 123
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria)
            ;

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                PageNumber = 0,
                PageSize = 0,
                ContactId = 123
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria);

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

        [Fact]
        public async Task GetContactsAccountByAdminAsync_WhenValidContactId_ShouldReturnsContacts()
        {
            // Arrange
            var contactId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountByAdminAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expected);

            var accountService = new AccountService(accountRepository.Object);

            // Act
            var result = await accountService.GetContactsAccountByAdminAsync(string.Empty, contactId, 1, 999);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetContactsAccountByAdminAsync_WhenInvalidContactId_ShouldThrows_NotFoundException()
        {
            // Arrange
            var contactId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountByAdminAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new NotFoundException(Errors.NotFoundRoleContactCode, Errors.NotFoundRoleContactMessage));

            var accountService = new AccountService(accountRepository.Object);

            // Act
            var result = async () => await accountService.GetContactsAccountByAdminAsync(string.Empty, contactId, 1, 999);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundRoleContactCode, exception.Code);
            Assert.Equal(Errors.NotFoundRoleContactMessage, exception.Message);
        }
    }
}
