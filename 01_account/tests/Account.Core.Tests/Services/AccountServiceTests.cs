// <copyright file="AccountServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Core.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepository;
        private readonly Mock<IAccountEventPublisher> _accountEventPublisher;

        private readonly Fixture _fixture;

        public AccountServiceTests()
        {
            _accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            _accountEventPublisher = new Mock<IAccountEventPublisher>();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetAccountsAsync_NotEmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                Search = string.Empty,
                ContactId = 123
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria, pagination)
            ;

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                ContactId = 123
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria, null!);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAllAccountsAsync_NotEmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAllAccountsAsync("123456789", pagination)
            ;

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAllAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);

            // Act
            var accounts = await accountService.GetAllAccountsAsync("123456789", null!);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAllAccountsAsync_EmptyAccountNumber_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAllAccountsAsync(null, pagination);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task Should_GetAccountSummary_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountModel>();
            _accountRepository.Setup(repository => repository.GetAccountSummaryAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);

            // Act
            var accountSummary = await accountService.GetAccountSummaryAsync(accountId: 1);

            // Assert
            Assert.Equal(accountMocked, accountSummary);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);

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

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);

            // Act
            var accounts = await accountService.GetAccountAsync(accountId: 1);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task Should_UpdateAccount_ReturnsOkResultAsync_And_PublishEvent()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>()))
                .ReturnsAsync(It.IsAny<AccountDetail>());

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);

            // Act
            await accountService.UpdateAccountAsync(accountId: 1, accountMocked);

            // Assert
            _accountRepository.Verify(repository => repository.UpdateAccountAsync(1, accountMocked));
            _accountEventPublisher.Verify(e => e.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
        }

        [Fact]
        public async Task GetContactsAccountAsync_WhenNotEmptyAccountId_ShouldReturnsContacts()
        {
            // Arrange
            var accountId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<SearchContactsAccountCriteria>(), It.IsAny<Pagination>()))
                .Callback<int, SearchContactsAccountCriteria, Pagination>((accId, criteria, pagination) => accId.Should().Be(accountId))
                .ReturnsAsync(expected);

            var accountService = new AccountService(accountRepository.Object, _accountEventPublisher.Object);
            var searchCriteria = new SearchContactsAccountCriteria
            {
                Search = string.Empty,
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var result = await accountService.GetContactsAccountAsync(accountId, searchCriteria, pagination);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetContactsAccountAsync_WhenInvalidAccountId_ShouldThrow_NotFoundException()
        {
            // Arrange
            var accountId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<SearchContactsAccountCriteria>(), It.IsAny<Pagination>()))
                .Throws(new NotFoundException(Errors.NotFoundContactsCode, Errors.NotFoundContactsMessage));

            var accountService = new AccountService(accountRepository.Object, _accountEventPublisher.Object);
            var searchCriteria = new SearchContactsAccountCriteria
            {
                Search = string.Empty,
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var result = async () => await accountService.GetContactsAccountAsync(accountId, searchCriteria, pagination);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundContactsCode, exception.Code);
            Assert.Equal(Errors.NotFoundContactsMessage, exception.Message);
        }

        [Fact]
        public async Task GetAssociatedContactsAsync_WhenValidContactId_ShouldReturnsContacts()
        {
            // Arrange
            var contactId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();
            var request = new GetAssociatedContactsRequest
            {
                Search = string.Empty,
                ContactType = ContactType.Customer,
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 999
            };

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetAssociatedContactsAsync(contactId, request, pagination))
                .ReturnsAsync(expected);

            var accountService = new AccountService(accountRepository.Object, _accountEventPublisher.Object);

            // Act
            var result = await accountService.GetAssociatedContactsAsync(contactId, request, pagination);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetAssociatedContactsAsync_WhenInvalidContactId_ShouldThrows_NotFoundException()
        {
            // Arrange
            var contactId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();
            var request = new GetAssociatedContactsRequest
            {
                Search = string.Empty,
                ContactType = ContactType.Customer,
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 999
            };

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetAssociatedContactsAsync(contactId, request, pagination))
                .Throws(new NotFoundException(Errors.NotFoundRoleContactCode, Errors.NotFoundRoleContactMessage));

            var accountService = new AccountService(accountRepository.Object, _accountEventPublisher.Object);

            // Act
            var result = async () => await accountService.GetAssociatedContactsAsync(contactId, request, pagination);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundRoleContactCode, exception.Code);
            Assert.Equal(Errors.NotFoundRoleContactMessage, exception.Message);
        }

        [Fact]
        public async Task ShouldReturn_ExpectedOfficeId_WithExpectedValue_GetAccountsAsyncInvoked()
        {
            // Arrange
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                Search = string.Empty,
                ContactId = 123
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria, pagination);

            // Assert
            Assert.Equal(accountMocked, accounts);
            Assert.NotNull(accounts.Items);
            Assert.All(accounts.Items, account =>
            {
                Assert.NotEqual(0, account.OfficeId);
                Assert.NotNull(account.Office);
            });
        }

        [Fact]
        public async Task ShouldReturn_NullOfficeId_WithInvalidValue_GetAccountsAsyncInvoked()
        {
            // Arrange
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            var expectedAccount = this._fixture.CreateMany<AccountModel>().ToList();
            expectedAccount.ForEach(item =>
            {
                item.OfficeId = null;
                item.Office = null;
            });
            accountMocked.Items = expectedAccount;

            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _accountEventPublisher.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                Search = string.Empty,
                ContactId = 123
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria, pagination);

            // Assert
            Assert.Equal(accountMocked, accounts);
            Assert.NotNull(accounts.Items);
            Assert.All(accounts.Items, account =>
            {
                Assert.Null(account.OfficeId);
                Assert.Null(account.Office);
            });
        }
    }
}
