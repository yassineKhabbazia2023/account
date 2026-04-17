// <copyright file="AccountServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private readonly Mock<IContactRepository> _contactRepository;
        private readonly Mock<IAccountEventPublisher> _accountEventPublisher;
        private readonly ILogger<AccountService> _logger;

        private readonly Fixture _fixture;

        public AccountServiceTests()
        {
            _accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            _contactRepository = new Mock<IContactRepository>();
            _accountEventPublisher = new Mock<IAccountEventPublisher>();
            _logger = NullLogger<AccountService>.Instance;
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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
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
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>(), It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAllAccountsAsync("123456789", pagination, null!)
            ;

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAllAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>(), It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            var accounts = await accountService.GetAllAccountsAsync("123456789", null!, null!);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task GetAllAccountsAsync_EmptyAccountNumber_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>(), It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            // Act
            var accounts = await accountService.GetAllAccountsAsync(null, pagination, null!);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        [Fact]
        public async Task Should_GetAccountSummary_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountModel>();
            _accountRepository.Setup(repository => repository.GetAccountSummaryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            var accountSummary = await accountService.GetAccountSummaryAsync(1, 1);

            // Assert
            Assert.Equal(accountMocked, accountSummary);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

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
            _accountRepository.Setup(repository => repository.GetAccountAsync(It.IsAny<int>()))
                .ReturnsAsync(accountMocked);
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await accountService.UpdateAccountAsync(accountId: 1, accountMocked);

            // Assert
            _accountRepository.Verify(repository => repository.UpdateAccountAsync(1, accountMocked));
            _accountEventPublisher.Verify(e => e.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
        }

        [Fact]
        public async Task Should_CreateAccount_ReturnsCreatedAccount_And_PublishEvent()
        {
            // Arrange
            var currentUserId = 1;
            var request = new CreateAccountRequest
            {
                AccountNumber = "A12345",
                LegalName = "Account Test",
                Siret = "12345678900000"
            };

            var currentContact = _fixture.Build<Contact>().With(c => c.Email, "test@pulse.fr").Create();
            _contactRepository.Setup(c => c.GetContactByIdAsync(currentUserId)).ReturnsAsync(currentContact);

            var created = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.CreateAccountAsync(currentContact.Email, request)).ReturnsAsync(created);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            var result = await accountService.CreateAccountAsync(currentUserId, request);

            // Assert
            result.Should().Be(created);
            _accountEventPublisher.Verify(e => e.PublishAccountCreatedEventAsync(created), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var currentUserId = 999;
            var request = new CreateAccountRequest
            {
                AccountNumber = "A12345",
                LegalName = "Account Test",
                Siret = "12345678900000"
            };

            _contactRepository.Setup(c => c.GetContactByIdAsync(currentUserId)).ReturnsAsync((Contact)null!);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            var result = async () => await accountService.CreateAccountAsync(currentUserId, request);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundContactCode, exception.Code);
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

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
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

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
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

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

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

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);
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

        [Fact]
        public async Task UpdateAccountAsync_AllFieldsFilled_ShouldPass()
        {
            // Arrange
            var accountDetail = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789" },
                Phone = new List<Phone> { new Phone { PhoneNumber = "0625569262" } }
            };
            _accountRepository.Setup(repo => repo.GetAccountAsync(It.IsAny<int>()))
                .ReturnsAsync(accountDetail);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>()))
                .ReturnsAsync(accountDetail);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act & Assert
            await service.UpdateAccountAsync(1, accountDetail); // Doit passer sans exception
        }

        [Fact]
        public async Task UpdateAccountAsync_WhenStaffSizeRangeCleared_ShouldPreserveExistingValue()
        {
            // Arrange
            var currentAccount = new AccountDetail
            {
                AccountId = 1,
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = "10-50" },
                Phone = new List<Phone>()
            };
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = null },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repo => repo.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>()))
                .ReturnsAsync(currentAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Equal("10-50", updateAccount.Legal.StaffSizeRange);
        }

        [Fact]
        public async Task UpdateAccountAsync_WhenAccountingTypeCleared_ShouldPreserveExistingValue()
        {
            // Arrange
            var currentAccount = new AccountDetail
            {
                AccountId = 1,
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789" },
                Accounting = new Accounting { AccountingType = "Engagement" },
                Phone = new List<Phone>()
            };
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789" },
                Accounting = new Accounting { AccountingType = null },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repo => repo.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>()))
                .ReturnsAsync(currentAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Equal("Engagement", updateAccount.Accounting.AccountingType);
        }

        [Fact]
        public async Task UpdateAccountAsync_WhenBothFieldsCleared_ShouldPreserveBothExistingValues()
        {
            // Arrange
            var currentAccount = new AccountDetail
            {
                AccountId = 1,
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = "10-50" },
                Accounting = new Accounting { AccountingType = "Engagement" },
                Phone = new List<Phone>()
            };
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = "" },
                Accounting = new Accounting { AccountingType = "" },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repo => repo.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>()))
                .ReturnsAsync(currentAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Equal("10-50", updateAccount.Legal.StaffSizeRange);
            Assert.Equal("Engagement", updateAccount.Accounting.AccountingType);
        }

        [Fact]
        public async Task UpdateAccountAsync_WhenNewValuesProvided_ShouldUseNewValues()
        {
            // Arrange
            var currentAccount = new AccountDetail
            {
                AccountId = 1,
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = "10-50" },
                Accounting = new Accounting { AccountingType = "Engagement" },
                Phone = new List<Phone>()
            };
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = "50-100" },
                Accounting = new Accounting { AccountingType = "Tresorerie" },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repo => repo.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>()))
                .ReturnsAsync(updateAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Equal("50-100", updateAccount.Legal.StaffSizeRange);
            Assert.Equal("Tresorerie", updateAccount.Accounting.AccountingType);
        }

        [Fact]
        public async Task UpdateAccountAsync_WhenCurrentAccountIsNull_ShouldNotProtectFields()
        {
            // Arrange
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = null },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repo => repo.GetAccountAsync(1))
                .ReturnsAsync((AccountDetail?)null);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>()))
                .ReturnsAsync(updateAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Null(updateAccount.Legal.StaffSizeRange);
            _accountRepository.Verify(repo => repo.UpdateAccountAsync(1, updateAccount), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountAsync_WhenCurrentFieldsAreNull_ShouldAllowNullValues()
        {
            // Arrange
            var currentAccount = new AccountDetail
            {
                AccountId = 1,
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = null },
                Accounting = new Accounting { AccountingType = null },
                Phone = new List<Phone>()
            };
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal { LegalName = "SAS TEST", Siren = "123456789", StaffSizeRange = null },
                Accounting = new Accounting { AccountingType = null },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repo => repo.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>()))
                .ReturnsAsync(updateAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Null(updateAccount.Legal.StaffSizeRange);
            Assert.Null(updateAccount.Accounting.AccountingType);
        }
    }
}
