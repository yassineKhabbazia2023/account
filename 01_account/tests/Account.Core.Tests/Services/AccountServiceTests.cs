// <copyright file="AccountServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pulse.Account.Core.Constants;
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
        private readonly Mock<IFeatureFlagService> _featureFlagService;
        private readonly Mock<IRoleRepository> _roleRepository;

        private readonly Fixture _fixture;

        public AccountServiceTests()
        {
            _accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            _contactRepository = new Mock<IContactRepository>();
            _accountEventPublisher = new Mock<IAccountEventPublisher>();
            _logger = NullLogger<AccountService>.Instance;
            _featureFlagService = new Mock<IFeatureFlagService>();
            _featureFlagService.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _roleRepository = new Mock<IRoleRepository>();
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetAccountsAsync_NotEmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var searchAccountCriteria = new SearchAccountCriteria
            {
                ContactId = 123
            };

            // Act
            var accounts = await accountService.GetAccountsAsync(searchAccountCriteria, null!);

            // Assert
            Assert.Equal(accountMocked, accounts);
        }

        #region GetAccountsAsync coverage additions

        /// <summary>
        /// Ensures null criteria and pagination are normalized before reaching the repository.
        /// </summary>
        [Fact]
        public async Task GetAccountsAsync_WhenCriteriaAndPaginationAreNull_ShouldNormalizeInputsBeforeCallingRepository()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            SearchAccountCriteria? capturedCriteria = null;
            Pagination? capturedPagination = null;

            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .Callback<SearchAccountCriteria, Pagination, bool>((criteria, pagination, sortByLastActivity) =>
                {
                    capturedCriteria = criteria;
                    capturedPagination = pagination;
                })
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            var accounts = await accountService.GetAccountsAsync(null!, null);

            Assert.Equal(accountMocked, accounts);
            Assert.NotNull(capturedCriteria);
            Assert.NotNull(capturedPagination);
            Assert.Equal(1, capturedPagination!.PageNumber);
            Assert.Equal(int.MaxValue, capturedPagination.PageSize);
            Assert.Equal(0, capturedCriteria!.ContactId);
            Assert.Null(capturedCriteria.DeploymentStatus);
        }

        /// <summary>
        /// Ensures invalid deployment status values are rejected before repository execution.
        /// </summary>
        [Fact]
        public async Task GetAccountsAsync_WhenDeploymentStatusIsInvalid_ShouldThrowBadRequestException()
        {
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var criteria = new SearchAccountCriteria
            {
                ContactId = 123,
                DeploymentStatus = new List<int> { int.MaxValue }
            };

            var action = async () => await accountService.GetAccountsAsync(criteria, new Pagination());

            var exception = await Assert.ThrowsAsync<BadRequestException>(action);
            Assert.Equal(Errors.BadRequestDeploymentStatusCode, exception.Code);
            _accountRepository.Verify(
                repository => repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()),
                Times.Never);
        }

        /// <summary>
        /// Ensures invalid mission type values are rejected before repository execution.
        /// </summary>
        [Fact]
        public async Task GetAccountsAsync_WhenMissionTypeIsInvalid_ShouldThrowBadRequestException()
        {
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var criteria = new SearchAccountCriteria
            {
                ContactId = 123,
                MissionType = new List<string> { "Invalide" }
            };

            var action = async () => await accountService.GetAccountsAsync(criteria, new Pagination());

            var exception = await Assert.ThrowsAsync<BadRequestException>(action);
            Assert.Equal(Errors.BadRequestMissionTypeCode, exception.Code);
            _accountRepository.Verify(
                repository => repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()),
                Times.Never);
        }

        /// <summary>
        /// Ensures a valid mission type is forwarded unchanged to the repository.
        /// </summary>
        [Theory]
        [InlineData("Tenue")]
        [InlineData("Revision")]
        public async Task GetAccountsAsync_WhenMissionTypeIsValid_ShouldForwardItToRepository(string missionType)
        {
            SearchAccountCriteria? capturedCriteria = null;
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .Callback<SearchAccountCriteria, Pagination, bool>((criteria, pagination, sortByLastActivity) => capturedCriteria = criteria)
                .ReturnsAsync(_fixture.Create<Paging<AccountModel>>());

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var inputCriteria = new SearchAccountCriteria
            {
                ContactId = 123,
                MissionType = new List<string> { missionType }
            };

            await accountService.GetAccountsAsync(inputCriteria, new Pagination());

            Assert.NotNull(capturedCriteria);
            Assert.Equal(new[] { missionType }, capturedCriteria!.MissionType);
        }

        [Fact]
        public async Task GetAccountsAsync_WhenLastActivityDateFromIsAfterDateTo_ShouldThrowBadRequestException()
        {
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var criteria = new SearchAccountCriteria
            {
                ContactId = 123,
                LastActivityDateFrom = new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc),
                LastActivityDateTo = new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc)
            };

            var action = async () => await accountService.GetAccountsAsync(criteria, new Pagination());

            var exception = await Assert.ThrowsAsync<BadRequestException>(action);
            Assert.Equal(Errors.BadRequestLastActivityRangeCode, exception.Code);
            _accountRepository.Verify(
                repository => repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAccountsAsync_WhenLastActivityRangeIsValid_ShouldForwardItToRepository()
        {
            SearchAccountCriteria? capturedCriteria = null;
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .Callback<SearchAccountCriteria, Pagination, bool>((criteria, pagination, sortByLastActivity) => capturedCriteria = criteria)
                .ReturnsAsync(_fixture.Create<Paging<AccountModel>>());

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var from = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc);
            var inputCriteria = new SearchAccountCriteria
            {
                ContactId = 123,
                LastActivityDateFrom = from,
                LastActivityDateTo = to
            };

            await accountService.GetAccountsAsync(inputCriteria, new Pagination());

            Assert.NotNull(capturedCriteria);
            Assert.Equal(from, capturedCriteria!.LastActivityDateFrom);
            Assert.Equal(to, capturedCriteria.LastActivityDateTo);
        }

        [Fact]
        public async Task GetAccountsAsync_WhenLastActivityBoundsAreEqual_ShouldCallRepository()
        {
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .ReturnsAsync(_fixture.Create<Paging<AccountModel>>());

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var bound = new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc);
            var inputCriteria = new SearchAccountCriteria
            {
                ContactId = 123,
                LastActivityDateFrom = bound,
                LastActivityDateTo = bound
            };

            await accountService.GetAccountsAsync(inputCriteria, new Pagination());

            _accountRepository.Verify(
                repository => repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()),
                Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetAccountsAsync_ShouldForwardLastActivityFeatureFlagToRepository(bool lastActivityEnabled)
        {
            _featureFlagService.Setup(f => f.IsEnabledAsync(FeatureFlagKeys.LastActivityFeature, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(lastActivityEnabled);
            _accountRepository.Setup(repository =>
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .ReturnsAsync(_fixture.Create<Paging<AccountModel>>());
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            await accountService.GetAccountsAsync(new SearchAccountCriteria { ContactId = 123 }, new Pagination());

            _accountRepository.Verify(
                repository => repository.GetAccountsAsync(
                    It.IsAny<SearchAccountCriteria>(),
                    It.IsAny<Pagination>(),
                    lastActivityEnabled),
                Times.Once);
        }

        #endregion GetAccountsAsync coverage additions

        [Fact]
        public async Task GetAllAccountsAsync_NotEmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>(), It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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

        /// <summary>
        /// Ensures the contact widget service uses the dedicated non-paginated repository query.
        /// </summary>
        [Fact]
        public async Task GetAccountContactWidgetContactsAsync_Should_ReturnRepositoryContacts()
        {
            var accountId = 123;
            var expected = _fixture.CreateMany<Contact>(2).ToList();
            _accountRepository.Setup(repository => repository.GetAccountContactWidgetContactsAsync(accountId))
                .ReturnsAsync(expected);
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            var result = await accountService.GetAccountContactWidgetContactsAsync(accountId);

            Assert.Equal(expected, result);
            _accountRepository.Verify(repository => repository.GetAccountContactWidgetContactsAsync(accountId), Times.Once);
            _accountRepository.Verify(
                repository => repository.GetContactsAccountAsync(
                    It.IsAny<int>(),
                    It.IsAny<SearchContactsAccountCriteria>(),
                    It.IsAny<Pagination>(),
                    It.IsAny<bool>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAllAccountsAsync_EmptyPageNumberAndPageSize_ShouldReturnsAccounts()
        {
            var accountMocked = _fixture.Create<Paging<AccountModel>>();
            _accountRepository.Setup(repository =>
                    repository.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>(), It.IsAny<SearchAccountCriteria>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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
            _roleRepository.Setup(repo => repo.UpdateLastActivityDateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            var accountSummary = await accountService.GetAccountSummaryAsync(1, 1, "Collaborator");

            // Assert
            Assert.Equal(accountMocked, accountSummary);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.GetAccountDetailAsync(It.IsAny<int>())).ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            await accountService.UpdateAccountAsync(accountId: 1, accountMocked);

            // Assert
            _accountRepository.Verify(repository => repository.UpdateAccountAsync(1, accountMocked, It.IsAny<bool>()));
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
                Siret = "12345678900000",
                AccountType = AccountType.CLIENT
            };

            var currentContact = _fixture.Build<Contact>().With(c => c.Email, "test@pulse.fr").Create();
            _contactRepository.Setup(c => c.GetContactByIdAsync(currentUserId)).ReturnsAsync(currentContact);

            var created = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.CreateAccountAsync(currentContact.Email, request)).ReturnsAsync(created);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
                Siret = "12345678900000",
                AccountType = AccountType.CLIENT
            };

            _contactRepository.Setup(c => c.GetContactByIdAsync(currentUserId)).ReturnsAsync((Contact)null!);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            var result = async () => await accountService.CreateAccountAsync(currentUserId, request);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        }

        #region CreateAccountAsync coverage additions

        /// <summary>
        /// Ensures prospect account creation forwards the request and publishes the created-account event.
        /// </summary>
        [Fact]
        public async Task CreateAccountAsync_WhenRequestIsProspect_ShouldCreateAccountAndPublishEvent()
        {
            var currentUserId = 10;
            var request = new CreateAccountRequest
            {
                AccountNumber = "P12345",
                LegalName = "Prospect Test",
                Siret = "12345678900000",
                AccountType = AccountType.PROSPECT
            };
            var currentContact = _fixture.Build<Contact>().With(c => c.Email, "prospect.creator@pulse.fr").Create();
            var created = _fixture.Build<AccountDetail>()
                .With(c => c.AccountNumber, request.AccountNumber)
                .Create();

            _contactRepository.Setup(contactRepository => contactRepository.GetContactByIdAsync(currentUserId))
                .ReturnsAsync(currentContact);
            _accountRepository.Setup(repository => repository.CreateAccountAsync(currentContact.Email, request))
                .ReturnsAsync(created);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            var result = await accountService.CreateAccountAsync(currentUserId, request);

            result.Should().Be(created);
            _accountRepository.Verify(repository => repository.CreateAccountAsync(currentContact.Email, request), Times.Once);
            _accountEventPublisher.Verify(publisher => publisher.PublishAccountCreatedEventAsync(created), Times.Once);
        }

        /// <summary>
        /// Ensures the NAF code is normalized to the INSEE format (XX.XXZ) before reaching the repository.
        /// </summary>
        [Theory]
        [InlineData("6234Z", "62.34Z")]
        [InlineData("62.34Z", "62.34Z")]
        public async Task CreateAccountAsync_WithNafCode_ShouldFormatNafCodeBeforeCallingRepository(string inputNafCode, string expectedNafCode)
        {
            var currentUserId = 11;
            var request = new CreateAccountRequest
            {
                AccountNumber = "NAF12345",
                LegalName = "Naf Format Test",
                Siret = "12345678900000",
                AccountType = AccountType.CLIENT,
                NafCode = inputNafCode
            };
            var currentContact = _fixture.Build<Contact>().With(c => c.Email, "naf.creator@pulse.fr").Create();
            var created = _fixture.Create<AccountDetail>();

            _contactRepository.Setup(contactRepository => contactRepository.GetContactByIdAsync(currentUserId))
                .ReturnsAsync(currentContact);
            _accountRepository.Setup(repository => repository.CreateAccountAsync(currentContact.Email, It.IsAny<CreateAccountRequest>()))
                .ReturnsAsync(created);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            await accountService.CreateAccountAsync(currentUserId, request);

            _accountRepository.Verify(
                repository => repository.CreateAccountAsync(currentContact.Email, It.Is<CreateAccountRequest>(r => r.NafCode == expectedNafCode)),
                Times.Once);
        }

        /// <summary>
        /// Ensures create-account failures are logged with structured prospect-creation context and do not publish events.
        /// </summary>
        [Fact]
        public async Task CreateAccountAsync_WhenRepositoryCreateFails_ShouldLogStructuredErrorAndRethrow()
        {
            var currentUserId = 11;
            var request = new CreateAccountRequest
            {
                AccountNumber = "P54321",
                LegalName = "Broken Prospect",
                Siret = "99999999999999",
                AccountType = AccountType.PROSPECT
            };
            var currentContact = _fixture.Build<Contact>().With(c => c.Email, "creator@pulse.fr").Create();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var failure = new System.InvalidOperationException("repository failed");

            _contactRepository.Setup(contactRepository => contactRepository.GetContactByIdAsync(currentUserId))
                .ReturnsAsync(currentContact);
            _accountRepository.Setup(repository => repository.CreateAccountAsync(currentContact.Email, request))
                .ThrowsAsync(failure);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, loggerMock.Object, _featureFlagService.Object, _roleRepository.Object);

            var action = async () => await accountService.CreateAccountAsync(currentUserId, request);

            var exception = await Assert.ThrowsAsync<System.InvalidOperationException>(action);
            Assert.Same(failure, exception);
            _accountEventPublisher.Verify(publisher => publisher.PublishAccountCreatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);

            var invocation = Assert.Single(loggerMock.Invocations.Where(i => i.Method.Name == nameof(ILogger.Log)));
            Assert.Equal(LogLevel.Error, invocation.Arguments[0]);
            Assert.Same(failure, invocation.Arguments[3]);

            var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(invocation.Arguments[2]);
            Assert.Contains(state, item => item.Key == "ProspectCreationStep" && item.Value?.ToString() == "CreateRydgeAccountAsync");
            Assert.Contains(state, item => item.Key == "ServiceName" && item.Value?.ToString() == "Pulse.Back.Account");
            Assert.Contains(state, item => item.Key == "OperationName" && item.Value?.ToString() == "CreateAccountAsync");
            Assert.Contains(state, item => item.Key == "Siret" && item.Value?.ToString() == request.Siret);
            Assert.Contains(state, item => item.Key == "AccountNumber" && item.Value?.ToString() == request.AccountNumber);
            Assert.Contains(state, item => item.Key == "CurrentUserId" && item.Value?.ToString() == currentUserId.ToString());
        }

        #endregion CreateAccountAsync coverage additions

        [Fact]
        public async Task GetContactsAccountAsync_WhenNotEmptyAccountId_ShouldReturnsContacts()
        {
            // Arrange
            var accountId = 6000;
            var fixture = new Fixture();
            var expected = fixture.Create<Paging<Contact>>();

            var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
            accountRepository.Setup(repo => repo.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<SearchContactsAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .Callback<int, SearchContactsAccountCriteria, Pagination, bool>((accId, criteria, pagination, includeProspects) => accId.Should().Be(accountId))
                .ReturnsAsync(expected);

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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
            accountRepository.Setup(repo => repo.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<SearchContactsAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .Throws(new NotFoundException(Errors.NotFoundContactsCode, Errors.NotFoundContactsMessage));

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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

        #region IsContactProspectOnlyAsync

        /// <summary>
        /// Ensures the service returns the repository result for the prospect-only contact check.
        /// </summary>
        /// <param name="isProspectOnly">The repository result.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsContactProspectOnlyAsync_Should_Return_Repository_Result(bool isProspectOnly)
        {
            // Arrange
            var contactId = 6000;
            _accountRepository.Setup(repository => repository.IsContactProspectOnlyAsync(contactId))
                .ReturnsAsync(isProspectOnly);
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            var result = await accountService.IsContactProspectOnlyAsync(contactId);

            // Assert
            Assert.Equal(isProspectOnly, result);
            _accountRepository.Verify(repository => repository.IsContactProspectOnlyAsync(contactId), Times.Once);
        }

        /// <summary>
        /// Ensures the service preserves the existing contact-not-found behavior from the repository.
        /// </summary>
        [Fact]
        public async Task IsContactProspectOnlyAsync_WhenContactDoesNotExist_Should_Throw_NotFoundException()
        {
            // Arrange
            var contactId = 0;
            _accountRepository.Setup(repository => repository.IsContactProspectOnlyAsync(contactId))
                .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId)));
            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            var result = async () => await accountService.IsContactProspectOnlyAsync(contactId);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundContactCode, exception.Code);
            Assert.Equal(string.Format(Errors.NotFoundContactMessage, contactId), exception.Message);
        }

        #endregion IsContactProspectOnlyAsync

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

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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

            var accountService = new AccountService(accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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
                    repository.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(accountDetail);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(currentAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(currentAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(currentAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(updateAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(updateAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Null(updateAccount.Legal.StaffSizeRange);
            _accountRepository.Verify(repo => repo.UpdateAccountAsync(1, updateAccount, It.IsAny<bool>()), Times.Once);
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
            _accountRepository.Setup(repo => repo.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(updateAccount);

            var service = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            await service.UpdateAccountAsync(1, updateAccount);

            // Assert
            Assert.Null(updateAccount.Legal.StaffSizeRange);
            Assert.Null(updateAccount.Accounting.AccountingType);
        }

        #region UpdateAccountAsync coverage additions

        /// <summary>
        /// Ensures update failures do not publish update events.
        /// </summary>
        [Fact]
        public async Task UpdateAccountAsync_WhenRepositoryUpdateFails_ShouldNotPublishEvent()
        {
            var currentAccount = _fixture.Create<AccountDetail>();
            var accountToUpdate = _fixture.Create<AccountDetail>();

            _accountRepository.Setup(repository => repository.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(1, accountToUpdate, It.IsAny<bool>()))
                .ThrowsAsync(new NotFoundException(Errors.NotFoundAccountCode, Errors.NotFoundAccountMessage));

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            var action = async () => await accountService.UpdateAccountAsync(1, accountToUpdate);

            await Assert.ThrowsAsync<NotFoundException>(action);
            _accountEventPublisher.Verify(publisher => publisher.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
        }

        /// <summary>
        /// Ensures protected-field removals preserve values and emit structured error logs.
        /// </summary>
        [Fact]
        public async Task UpdateAccountAsync_WhenProtectedFieldsAreCleared_ShouldPreserveValuesAndLogStructuredErrors()
        {
            var loggerMock = new Mock<ILogger<AccountService>>();
            var currentAccount = new AccountDetail
            {
                AccountId = 1,
                AccountNumber = "A12345",
                Legal = new Legal
                {
                    LegalName = "SAS TEST",
                    Siren = "123456789",
                    StaffSizeRange = "10-50"
                },
                Accounting = new Accounting
                {
                    AccountingType = "Engagement"
                },
                Phone = new List<Phone>()
            };
            var updateAccount = new AccountDetail
            {
                AccountNumber = "A12345",
                Legal = new Legal
                {
                    LegalName = "SAS TEST",
                    Siren = "123456789",
                    StaffSizeRange = null
                },
                Accounting = new Accounting
                {
                    AccountingType = string.Empty
                },
                Phone = new List<Phone>()
            };

            _accountRepository.Setup(repository => repository.GetAccountAsync(1))
                .ReturnsAsync(currentAccount);
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(1, It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(updateAccount);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, loggerMock.Object, _featureFlagService.Object, _roleRepository.Object);

            await accountService.UpdateAccountAsync(1, updateAccount);

            Assert.Equal("10-50", updateAccount.Legal!.StaffSizeRange);
            Assert.Equal("Engagement", updateAccount.Accounting!.AccountingType);

            var logInvocations = loggerMock.Invocations
                .Where(invocation => invocation.Method.Name == nameof(ILogger.Log))
                .ToList();

            Assert.Equal(2, logInvocations.Count);

            var staffSizeLog = logInvocations.Single(invocation =>
            {
                var state = invocation.Arguments[2] as IReadOnlyList<KeyValuePair<string, object?>>;
                return state?.Any(log => log.Key == "{OriginalFormat}" &&
                    log.Value?.ToString() == "Tentative de suppression du champ StaffSizeRange pour le compte {AccountId}. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.") == true;
            });

            var staffSizeState = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(staffSizeLog.Arguments[2]);
            Assert.Equal(LogLevel.Error, staffSizeLog.Arguments[0]);
            Assert.Contains(staffSizeState, log => log.Key == "AccountId" && log.Value?.ToString() == "1");
            Assert.Contains(staffSizeState, log => log.Key == "CurrentValue" && log.Value?.ToString() == "10-50");

            var accountingLog = logInvocations.Single(invocation =>
            {
                var state = invocation.Arguments[2] as IReadOnlyList<KeyValuePair<string, object?>>;
                return state?.Any(log => log.Key == "{OriginalFormat}" &&
                    log.Value?.ToString() == "Tentative de suppression du champ AccountingType pour le compte {AccountId}. Valeur actuelle: {CurrentValue}. La valeur existante sera conservée.") == true;
            });

            var accountingState = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(accountingLog.Arguments[2]);
            Assert.Equal(LogLevel.Error, accountingLog.Arguments[0]);
            Assert.Contains(accountingState, log => log.Key == "AccountId" && log.Value?.ToString() == "1");
            Assert.Contains(accountingState, log => log.Key == "CurrentValue" && log.Value?.ToString() == "Engagement");
        }

        /// <summary>
        /// When the prospect experience flag is ON, the update must include prospect accounts.
        /// </summary>
        [Fact]
        public async Task UpdateAccountAsync_WhenProspectExperienceEnabled_ShouldUpdateIncludingProspects()
        {
            var accountMocked = _fixture.Create<AccountDetail>();
            _featureFlagService.Setup(f => f.IsEnabledAsync(FeatureFlagKeys.IncludeProspectsInContactsSearch, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _accountRepository.Setup(repository => repository.GetAccountAsync(It.IsAny<int>()))
                .ReturnsAsync(accountMocked);
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            await accountService.UpdateAccountAsync(1, accountMocked);

            _accountRepository.Verify(repository => repository.UpdateAccountAsync(1, accountMocked, true), Times.Once);
            _accountEventPublisher.Verify(e => e.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
        }

        /// <summary>
        /// When the prospect experience flag is OFF, the update must keep excluding prospect accounts.
        /// </summary>
        [Fact]
        public async Task UpdateAccountAsync_WhenProspectExperienceDisabled_ShouldUpdateExcludingProspects()
        {
            var accountMocked = _fixture.Create<AccountDetail>();
            _accountRepository.Setup(repository => repository.GetAccountAsync(It.IsAny<int>()))
                .ReturnsAsync(accountMocked);
            _accountRepository.Setup(repository => repository.UpdateAccountAsync(It.IsAny<int>(), It.IsAny<AccountDetail>(), It.IsAny<bool>()))
                .ReturnsAsync(accountMocked);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            await accountService.UpdateAccountAsync(1, accountMocked);

            _accountRepository.Verify(repository => repository.UpdateAccountAsync(1, accountMocked, false), Times.Once);
        }

        #endregion UpdateAccountAsync coverage additions

        [Fact]
        public async Task SearchAccountsAsync_WithNullPagination_ShouldUseDefaultPagination()
        {
            var expectedResult = _fixture.Create<Paging<AccountSearchResult>>();
            _accountRepository.Setup(repository =>
                    repository.SearchAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(expectedResult);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);

            // Act
            var result = await accountService.SearchAccountsAsync("test", null);

            // Assert
            Assert.Equal(expectedResult, result);
            _accountRepository.Verify(r => r.SearchAccountsAsync(
                "test",
                It.Is<Pagination>(p => p.PageNumber == 1 && p.PageSize == int.MaxValue)), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task SearchAccountsAsync_WithInvalidPageNumber_ShouldNormalizeToOne(int invalidPageNumber)
        {
            var expectedResult = _fixture.Create<Paging<AccountSearchResult>>();
            _accountRepository.Setup(repository =>
                    repository.SearchAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(expectedResult);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var pagination = new Pagination { PageNumber = invalidPageNumber, PageSize = 10 };

            // Act
            var result = await accountService.SearchAccountsAsync("test", pagination);

            // Assert
            Assert.Equal(expectedResult, result);
            _accountRepository.Verify(r => r.SearchAccountsAsync(
                "test",
                It.Is<Pagination>(p => p.PageNumber == 1)), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public async Task SearchAccountsAsync_WithInvalidPageSize_ShouldNormalizeToMaxValue(int invalidPageSize)
        {
            var expectedResult = _fixture.Create<Paging<AccountSearchResult>>();
            _accountRepository.Setup(repository =>
                    repository.SearchAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(expectedResult);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var pagination = new Pagination { PageNumber = 1, PageSize = invalidPageSize };

            // Act
            var result = await accountService.SearchAccountsAsync("test", pagination);

            // Assert
            Assert.Equal(expectedResult, result);
            _accountRepository.Verify(r => r.SearchAccountsAsync(
                "test",
                It.Is<Pagination>(p => p.PageSize == int.MaxValue)), Times.Once);
        }

        [Fact]
        public async Task SearchAccountsAsync_WithValidPagination_ShouldPassThrough()
        {
            var expectedResult = _fixture.Create<Paging<AccountSearchResult>>();
            _accountRepository.Setup(repository =>
                    repository.SearchAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>()))
                .ReturnsAsync(expectedResult);

            var accountService = new AccountService(_accountRepository.Object, _contactRepository.Object, _accountEventPublisher.Object, _logger, _featureFlagService.Object, _roleRepository.Object);
            var pagination = new Pagination { PageNumber = 2, PageSize = 50 };

            // Act
            var result = await accountService.SearchAccountsAsync("test", pagination);

            // Assert
            Assert.Equal(expectedResult, result);
            _accountRepository.Verify(r => r.SearchAccountsAsync(
                "test",
                It.Is<Pagination>(p => p.PageNumber == 2 && p.PageSize == 50)), Times.Once);
        }
    }
}
