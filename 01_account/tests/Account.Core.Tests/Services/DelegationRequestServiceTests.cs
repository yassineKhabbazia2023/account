// <copyright file="DelegationRequestServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Services;

public class DelegationRequestServiceTests
{
    private readonly Mock<IDelegationRequestRepository> _mockRepository;
    private readonly Mock<IDelegationService> _mockDelegationService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IContactRepository> _mockContactRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IDelegationRequestEventPublisher> _mockDelegationRequestEventPublisher;
    private readonly Mock<ILogger<DelegationRequestService>> _mockLogger;
    private readonly DelegationRequestService _service;

    public DelegationRequestServiceTests()
    {
        _mockRepository = new Mock<IDelegationRequestRepository>();
        _mockDelegationService = new Mock<IDelegationService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockContactRepository = new Mock<IContactRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockDelegationRequestEventPublisher = new Mock<IDelegationRequestEventPublisher>();
        _mockLogger = new Mock<ILogger<DelegationRequestService>>();

        _mockContactRepository
            .Setup(r => r.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Contact
            {
                ContactId = id,
                FirstName = $"First{id}",
                LastName = $"Last{id}",
                Email = $"contact{id}@pulse.test",
                Type = ContactType.Collaborator.ToString()
            });

        _mockAccountRepository
            .Setup(r => r.GetAccountAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new AccountDetail
            {
                AccountId = id,
                AccountNumber = $"ACC{id}",
                Legal = new Legal { LegalName = $"Legal{id}" },
                Phone = new List<Phone>()
            });

        _service = new DelegationRequestService(
            _mockRepository.Object,
            _mockDelegationService.Object,
            _mockEmailService.Object,
            _mockContactRepository.Object,
            _mockAccountRepository.Object,
            _mockDelegationRequestEventPublisher.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenAllValid_ShouldCreateRequests()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(3)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(2, request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(3, request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.IsAny<int[]>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        var result = await _service.CreateDelegationRequestsAsync(contactId, request);

        result.CreatedRecipientIds.Should().BeEquivalentTo(new[] { 2, 3 });
        result.Errors.Should().BeEmpty();
        _mockRepository.Verify(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.IsAny<int[]>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRequestorIsNotCollaborator_ShouldThrowUnauthorizedException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _mockContactRepository
            .Setup(r => r.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact
            {
                ContactId = contactId,
                FirstName = "First1",
                LastName = "Last1",
                Email = "contact1@pulse.test",
                Type = ContactType.Customer.ToString()
            });

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .Where(ex => ex.Code == Errors.BadRequestClientCannotRequestDelegationCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientNotFound_ShouldAddRecipientNotFoundErrorAndCreateOthers()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(3, request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.IsAny<int[]>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        _mockContactRepository
            .Setup(r => r.GetContactByIdAsync(2))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, 2)));

        var result = await _service.CreateDelegationRequestsAsync(contactId, request);

        result.CreatedRecipientIds.Should().BeEquivalentTo(new[] { 3 });
        result.Errors.Should().ContainSingle(e => e.RecipientId == 2 && e.Reason == "RecipientNotFound");
        _mockRepository.Verify(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 3 })), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientNotCollaborator_ShouldAddRecipientNotCollaboratorErrorAndCreateOthers()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(3, request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.IsAny<int[]>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        _mockContactRepository
            .Setup(r => r.GetContactByIdAsync(2))
            .ReturnsAsync(new Contact
            {
                ContactId = 2,
                FirstName = "First2",
                LastName = "Last2",
                Email = "contact2@pulse.test",
                Type = ContactType.Customer.ToString()
            });

        var result = await _service.CreateDelegationRequestsAsync(contactId, request);

        result.CreatedRecipientIds.Should().BeEquivalentTo(new[] { 3 });
        result.Errors.Should().ContainSingle(e => e.RecipientId == 2 && e.Reason == "RecipientNotCollaborator");
        _mockRepository.Verify(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.Is<int[]>(ids => ids.SequenceEqual(new[] { 3 })), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenAllRecipientsNotCollaborator_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);

        _mockContactRepository
            .Setup(r => r.GetContactByIdAsync(It.Is<int>(id => id == 2 || id == 3)))
            .ReturnsAsync((int id) => new Contact
            {
                ContactId = id,
                FirstName = $"First{id}",
                LastName = $"Last{id}",
                Email = $"contact{id}@pulse.test",
                Type = ContactType.Customer.ToString()
            });

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.RecipientDoesNotHaveAccessCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientIdsEmpty_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = Array.Empty<int>()
        };

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.CreateDelegationCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientIdsHasDuplicates_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 2, 3 }
        };

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.CreateDelegationCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenSelfDelegation_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 1, 2 }
        };

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.SelfDelegationRequestCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenAccountDoesNotExist_ShouldThrowNotFoundException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 999,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Code == Errors.NotFoundAccountCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientDoesNotExist_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.RecipientDoesNotHaveAccessCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRequesterAlreadyHasAccess_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(true);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.RequesterAlreadyHasAccessCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientDoesNotHaveAccess_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(2, request.AccountId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.RecipientDoesNotHaveAccessCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenPendingRequestExists_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(true);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestAlreadyPendingCode);
    }

    [Fact]
    public async Task GetSentRequestsAsync_WhenCalled_ShouldReturnPaginatedResults()
    {
        var contactId = 1;
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var expectedResult = new Paging<DelegationRequest>
        {
            Items = new List<DelegationRequest>(),
            CurrentPage = 1,
            TotalPage = 0,
            TotalItems = 0
        };

        _mockRepository.Setup(r => r.GetSentRequestsAsync(contactId, null, It.IsAny<Pagination>())).ReturnsAsync(expectedResult);

        var result = await _service.GetSentRequestsAsync(contactId, pagination);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetSentRequestsAsync(contactId, null, It.IsAny<Pagination>()), Times.Once);
    }

    [Fact]
    public async Task GetSentRequestsAsync_WhenPaginationIsNull_ShouldUseDefaults()
    {
        var contactId = 1;
        var expectedResult = new Paging<DelegationRequest>
        {
            Items = new List<DelegationRequest>(),
            CurrentPage = 1,
            TotalPage = 0,
            TotalItems = 0
        };

        _mockRepository.Setup(r => r.GetSentRequestsAsync(contactId, null, It.IsAny<Pagination>())).ReturnsAsync(expectedResult);

        var result = await _service.GetSentRequestsAsync(contactId, null);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetSentRequestsAsync(contactId, null, It.Is<Pagination>(p => p.PageNumber == 1 && p.PageSize == int.MaxValue)), Times.Once);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(1, 0, 1, int.MaxValue)]
    [InlineData(1, -5, 1, int.MaxValue)]
    [InlineData(1, 200, 1, 200)]
    public async Task GetSentRequestsAsync_WithVariousPaginationValues_ShouldNormalizeCorrectly(int pageNumber, int pageSize, int expectedPageNumber, int expectedPageSize)
    {
        var contactId = 1;
        var pagination = new Pagination { PageNumber = pageNumber, PageSize = pageSize };
        var expectedResult = new Paging<DelegationRequest>
        {
            Items = new List<DelegationRequest>(),
            CurrentPage = expectedPageNumber,
            TotalPage = 0,
            TotalItems = 0
        };

        _mockRepository.Setup(r => r.GetSentRequestsAsync(contactId, null, It.Is<Pagination>(p => p.PageNumber == expectedPageNumber && p.PageSize == expectedPageSize))).ReturnsAsync(expectedResult);

        var result = await _service.GetSentRequestsAsync(contactId, pagination);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetSentRequestsAsync(contactId, null, It.Is<Pagination>(p => p.PageNumber == expectedPageNumber && p.PageSize == expectedPageSize)), Times.Once);
    }

    [Fact]
    public async Task GetReceivedRequestsAsync_WhenStatusIsNull_ShouldDefaultToPending()
    {
        var contactId = 1;
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var expectedResult = new Paging<DelegationRequest>
        {
            Items = new List<DelegationRequest>(),
            CurrentPage = 1,
            TotalPage = 0,
            TotalItems = 0
        };

        _mockRepository.Setup(r => r.GetReceivedRequestsAsync(contactId, It.Is<string[]>(s => s.Contains("pending")), It.IsAny<Pagination>())).ReturnsAsync(expectedResult);

        var result = await _service.GetReceivedRequestsAsync(contactId, pagination, null);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetReceivedRequestsAsync(contactId, It.Is<string[]>(s => s.Contains("pending")), It.IsAny<Pagination>()), Times.Once);
    }

    [Fact]
    public async Task GetReceivedRequestsAsync_WhenPaginationIsNull_ShouldUseDefaults()
    {
        var contactId = 1;
        var expectedResult = new Paging<DelegationRequest>
        {
            Items = new List<DelegationRequest>(),
            CurrentPage = 1,
            TotalPage = 0,
            TotalItems = 0
        };

        _mockRepository.Setup(r => r.GetReceivedRequestsAsync(contactId, It.Is<string[]>(s => s.Contains("pending")), It.IsAny<Pagination>())).ReturnsAsync(expectedResult);

        var result = await _service.GetReceivedRequestsAsync(contactId, null, null);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetReceivedRequestsAsync(contactId, It.Is<string[]>(s => s.Contains("pending")), It.Is<Pagination>(p => p.PageNumber == 1 && p.PageSize == int.MaxValue)), Times.Once);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(1, 0, 1, int.MaxValue)]
    [InlineData(1, -5, 1, int.MaxValue)]
    [InlineData(1, 200, 1, 200)]
    public async Task GetReceivedRequestsAsync_WithVariousPaginationValues_ShouldNormalizeCorrectly(int pageNumber, int pageSize, int expectedPageNumber, int expectedPageSize)
    {
        var contactId = 1;
        var pagination = new Pagination { PageNumber = pageNumber, PageSize = pageSize };
        var expectedResult = new Paging<DelegationRequest>
        {
            Items = new List<DelegationRequest>(),
            CurrentPage = expectedPageNumber,
            TotalPage = 0,
            TotalItems = 0
        };

        _mockRepository.Setup(r => r.GetReceivedRequestsAsync(contactId, It.Is<string[]>(s => s.Contains("pending")), It.Is<Pagination>(p => p.PageNumber == expectedPageNumber && p.PageSize == expectedPageSize))).ReturnsAsync(expectedResult);

        var result = await _service.GetReceivedRequestsAsync(contactId, pagination, null);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetReceivedRequestsAsync(contactId, It.Is<string[]>(s => s.Contains("pending")), It.Is<Pagination>(p => p.PageNumber == expectedPageNumber && p.PageSize == expectedPageSize)), Times.Once);
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenAccountIdIsZero_ShouldThrowBadRequestException()
    {
        var contactId = 1;
        var accountId = 0;

        Func<Task> act = async () => await _service.CheckEligibilityAsync(contactId, accountId);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenAccountDoesNotExist_ShouldThrowNotFoundException()
    {
        var contactId = 1;
        var accountId = 999;

        _mockRepository.Setup(r => r.DoesAccountExistAsync(accountId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CheckEligibilityAsync(contactId, accountId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenUserHasAccess_ShouldReturnAlreadyInPortfolio()
    {
        var contactId = 1;
        var accountId = 100;

        _mockRepository.Setup(r => r.DoesAccountExistAsync(accountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, accountId)).ReturnsAsync(true);

        var result = await _service.CheckEligibilityAsync(contactId, accountId);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Be(DelegationEligibilityReason.AlreadyInPortfolio);
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenPendingRequestExists_ShouldReturnAlreadyPending()
    {
        var contactId = 1;
        var accountId = 100;

        _mockRepository.Setup(r => r.DoesAccountExistAsync(accountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, accountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, accountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, accountId)).ReturnsAsync(true);

        var result = await _service.CheckEligibilityAsync(contactId, accountId);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Be(DelegationEligibilityReason.AlreadyPending);
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenEligible_ShouldReturnEligibleWithNoReason()
    {
        var contactId = 1;
        var accountId = 100;

        _mockRepository.Setup(r => r.DoesAccountExistAsync(accountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, accountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, accountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, accountId)).ReturnsAsync(false);

        var result = await _service.CheckEligibilityAsync(contactId, accountId);

        result.IsEligible.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    // ========== AcceptRequestsAsync ==========

    [Fact]
    public async Task AcceptRequestsAsync_WhenRequestIdsEmpty_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = Array.Empty<int>() };

        Func<Task> act = async () => await _service.AcceptRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestIdsEmptyCode);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenAllIdsNotFound_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 999 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>());

        Func<Task> act = async () => await _service.AcceptRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenOneIdInvalid_ShouldProcessOthersAndReturnError()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 999 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 })), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        var result = await _service.AcceptRequestsAsync(currentUserId, request);

        result.ProcessedIds.Should().ContainSingle().Which.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].DelegationRequestId.Should().Be(999);
        result.Errors[0].Reason.Should().Be("InvalidRequest");
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenNoValidRequestsFound_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>());

        Func<Task> act = async () => await _service.AcceptRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestAllInvalidCode);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenValid_ShouldCallRepositoryAcceptAndReturnProcessedIds()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        var result = await _service.AcceptRequestsAsync(currentUserId, request);

        result.ProcessedIds.Should().BeEquivalentTo(new[] { 1, 2 });
        result.Errors.Should().BeEmpty();
        _mockRepository.Verify(r => r.AcceptRequestsAsync(It.Is<int[]>(ids => ids.Length == 2), It.IsAny<DateTime>()), Times.Once);
        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.IsAny<CreateDelegationRequest>()), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenValid_ShouldCreateDelegationForRequester()
    {
        var currentUserId = 10;
        var requesterId = 5;
        var accountId = 100;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = requesterId, AccountId = accountId, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.Is<CreateDelegationRequest>(d => d.AccountIds!.Contains(accountId) && d.DelegationDetails.Any(dd => dd.DelegateeId == requesterId) && !d.IsFullDelegation)), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenMultipleDistinctPairs_ShouldCreateDelegationForEachPair()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 7, AccountId = 200, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.Is<CreateDelegationRequest>(d => d.AccountIds!.Contains(100) && d.DelegationDetails.Any(dd => dd.DelegateeId == 5))), Times.Once);
        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.Is<CreateDelegationRequest>(d => d.AccountIds!.Contains(200) && d.DelegationDetails.Any(dd => dd.DelegateeId == 7))), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenSameRequesterAndAccount_ShouldCreateDelegationOnlyOnce()
    {
        var currentUserId = 10;
        var requesterId = 5;
        var accountId = 100;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = requesterId, AccountId = accountId, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = requesterId, AccountId = accountId, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.IsAny<CreateDelegationRequest>()), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenValid_ShouldCreateDelegationWithCorrectStartDate()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.Is<CreateDelegationRequest>(d => d.DelegationDetails.All(dd => dd.StartDate != null && dd.StartDate.Value.Date == DateTime.UtcNow.Date) && d.DelegationDetails.All(dd => !dd.IsAutomaticDelegation))), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenDelegationServiceThrows_ShouldPropagateException()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>()))
            .ThrowsAsync(new BadRequestException("ERR", "Delegation creation failed"));

        Func<Task> act = async () => await _service.AcceptRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    // ========== AcceptRequestsAsync notification ==========
    [Fact]
    public async Task AcceptRequestsAsync_WhenValid_ShouldPublishValidatedEventWithAllRecipients()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };
        var acceptedSiblingRequests = new List<DelegationRequest>
        {
            new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "accepted" },
            new DelegationRequest { DelegationRequestId = 2, RecipientId = 11, RequesterId = 5, AccountId = 100, Status = "accepted" },
            new DelegationRequest { DelegationRequestId = 3, RecipientId = 12, RequesterId = 5, AccountId = 100, Status = "accepted" }
        };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.GetAcceptedSiblingRequestsAsync(5, 100, It.IsAny<DateTime>())).ReturnsAsync(acceptedSiblingRequests);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockDelegationRequestEventPublisher.Verify(
            p => p.PublishDelegationRequestValidatedEventAsync(currentUserId, It.Is<List<DelegationRequest>>(requests => requests.SequenceEqual(acceptedSiblingRequests))),
            Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenMultipleDistinctPairs_ShouldPublishValidatedEventForEachPair()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 7, AccountId = 200, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.GetAcceptedSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "accepted" }
            });
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockDelegationRequestEventPublisher.Verify(p => p.PublishDelegationRequestValidatedEventAsync(currentUserId, It.IsAny<List<DelegationRequest>>()), Times.Exactly(2));
    }

    // ========== AcceptSiblingRequestsAsync coverage ==========

    [Fact]
    public async Task AcceptRequestsAsync_WhenValid_ShouldCallAcceptSiblingRequestsForEachPair()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockRepository.Verify(r => r.AcceptSiblingRequestsAsync(5, 100, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenMultipleDistinctPairs_ShouldCallAcceptSiblingRequestsForEach()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 7, AccountId = 200, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockRepository.Verify(r => r.AcceptSiblingRequestsAsync(5, 100, It.IsAny<DateTime>()), Times.Once);
        _mockRepository.Verify(r => r.AcceptSiblingRequestsAsync(7, 200, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenSameRequesterAndAccount_ShouldCallAcceptSiblingRequestsOnlyOnce()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockRepository.Verify(r => r.AcceptSiblingRequestsAsync(5, 100, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenAcceptSiblingRequestsThrows_ShouldPropagateException()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).ThrowsAsync(new System.InvalidOperationException("DB error"));
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        Func<Task> act = async () => await _service.AcceptRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<System.InvalidOperationException>().WithMessage("DB error");
    }

    [Fact]
    public async Task AcceptRequestsAsync_ShouldCallAcceptSiblingRequestsWithSameRespondedAtAsAcceptRequests()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };
        DateTime capturedAcceptDate = default;
        DateTime capturedSiblingDate = default;

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Callback<int[], DateTime>((_, d) => capturedAcceptDate = d).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Callback<int, int, DateTime>((_, _, d) => capturedSiblingDate = d).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        capturedAcceptDate.Should().Be(capturedSiblingDate);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenSiblingRequestsSucceed_ShouldStillCreateDelegation()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.AcceptRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AcceptSiblingRequestsAsync(5, 100, It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockDelegationService.Setup(s => s.CreateDelegationAsync(It.IsAny<int>(), It.IsAny<CreateDelegationRequest>())).Returns(Task.CompletedTask);

        await _service.AcceptRequestsAsync(currentUserId, request);

        _mockRepository.Verify(r => r.AcceptSiblingRequestsAsync(5, 100, It.IsAny<DateTime>()), Times.Once);
        _mockDelegationService.Verify(s => s.CreateDelegationAsync(currentUserId, It.Is<CreateDelegationRequest>(d => d.AccountIds!.Contains(100) && d.DelegationDetails.Any(dd => dd.DelegateeId == 5))), Times.Once);
    }

    // ========== RefuseRequestsAsync ==========

    [Fact]
    public async Task RefuseRequestsAsync_WhenRequestIdsEmpty_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = Array.Empty<int>() };

        Func<Task> act = async () => await _service.RefuseRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestIdsEmptyCode);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenNoValidRequestsFound_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 999 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>());

        Func<Task> act = async () => await _service.RefuseRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestAllInvalidCode);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenOneIdInvalid_ShouldProcessOthersAndReturnError()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, Status = "pending" }
            });
        _mockRepository.Setup(r => r.RefuseRequestsAsync(It.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 })), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

        var result = await _service.RefuseRequestsAsync(currentUserId, request);

        result.ProcessedIds.Should().ContainSingle().Which.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].DelegationRequestId.Should().Be(2);
        result.Errors[0].Reason.Should().Be("InvalidRequest");
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenValid_ShouldCallRepositoryRefuseAndReturnProcessedIds()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, Status = "pending" }
            });
        _mockRepository.Setup(r => r.RefuseRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

        var result = await _service.RefuseRequestsAsync(currentUserId, request);

        result.ProcessedIds.Should().ContainSingle().Which.Should().Be(1);
        result.Errors.Should().BeEmpty();
        _mockRepository.Verify(r => r.RefuseRequestsAsync(It.Is<int[]>(ids => ids.Length == 1), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestsAsync_WhenRequestIdsNull_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = null! };

        Func<Task> act = async () => await _service.AcceptRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestIdsEmptyCode);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenRequestIdsNull_ShouldThrowBadRequestException()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = null! };

        Func<Task> act = async () => await _service.RefuseRequestsAsync(currentUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.DelegationRequestIdsEmptyCode);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenAllSiblingsRefused_ShouldReturnRequesterInAllRefusedRequesterIds()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: true);

        var result = await _service.RefuseRequestsAsync(currentUserId, request);

        result.AllRefusedRequesterIds.Should().ContainSingle().Which.Should().Be(5);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenAllSiblingsRefused_ShouldSendRefusedEmailToRequesterWithAllRefusers()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: true);

        await _service.RefuseRequestsAsync(currentUserId, request);

        _mockEmailService.Verify(
            e => e.SendDelegationRequestRefusedEmailAsync(It.Is<Contact>(c => c.ContactId == 5), It.Is<AccountDetail>(a => a.AccountId == 100), It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 10, 11 }))),
            Times.Once);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenAllSiblingsRefused_ShouldPublishRefusedEvent()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: true);

        await _service.RefuseRequestsAsync(currentUserId, request);

        _mockDelegationRequestEventPublisher.Verify(
            p => p.PublishDelegationRequestRefusedEventAsync(currentUserId, 5, 100, "REGULAR"),
            Times.Once);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenSiblingsStillPending_ShouldNotNotifyRequester()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: false);

        var result = await _service.RefuseRequestsAsync(currentUserId, request);

        result.AllRefusedRequesterIds.Should().BeEmpty();
        _mockEmailService.Verify(e => e.SendDelegationRequestRefusedEmailAsync(It.IsAny<Contact>(), It.IsAny<AccountDetail>(), It.IsAny<IEnumerable<int>>()), Times.Never);
        _mockDelegationRequestEventPublisher.Verify(p => p.PublishDelegationRequestRefusedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenNotificationFails_ShouldStillReturnProcessedIdsAndLogError()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: true);
        _mockEmailService
            .Setup(e => e.SendDelegationRequestRefusedEmailAsync(It.IsAny<Contact>(), It.IsAny<AccountDetail>(), It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(new System.InvalidOperationException("bus down"));

        var result = await _service.RefuseRequestsAsync(currentUserId, request);

        result.ProcessedIds.Should().ContainSingle().Which.Should().Be(1);
        result.AllRefusedRequesterIds.Should().ContainSingle().Which.Should().Be(5);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<System.InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenNoRefusedSiblingFound_ShouldNotNotifyRequester()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: true);
        _mockRepository.Setup(r => r.GetRefusedSiblingRequestsAsync(5, 100)).ReturnsAsync(new List<DelegationRequest>());

        await _service.RefuseRequestsAsync(currentUserId, request);

        _mockEmailService.Verify(e => e.SendDelegationRequestRefusedEmailAsync(It.IsAny<Contact>(), It.IsAny<AccountDetail>(), It.IsAny<IEnumerable<int>>()), Times.Never);
        _mockDelegationRequestEventPublisher.Verify(p => p.PublishDelegationRequestRefusedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenMultipleDistinctPairsAllRefused_ShouldNotifyEachRequester()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" },
                new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 7, AccountId = 200, Status = "pending" }
            });
        _mockRepository.Setup(r => r.RefuseRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AreAllSiblingRequestsRefusedAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetRefusedSiblingRequestsAsync(5, 100)).ReturnsAsync(new List<DelegationRequest>
        {
            new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "refused" }
        });
        _mockRepository.Setup(r => r.GetRefusedSiblingRequestsAsync(7, 200)).ReturnsAsync(new List<DelegationRequest>
        {
            new DelegationRequest { DelegationRequestId = 2, RecipientId = 10, RequesterId = 7, AccountId = 200, Status = "refused" }
        });

        var result = await _service.RefuseRequestsAsync(currentUserId, request);

        result.AllRefusedRequesterIds.Should().BeEquivalentTo(new[] { 5, 7 });
        _mockEmailService.Verify(e => e.SendDelegationRequestRefusedEmailAsync(It.Is<Contact>(c => c.ContactId == 5), It.Is<AccountDetail>(a => a.AccountId == 100), It.IsAny<IEnumerable<int>>()), Times.Once);
        _mockEmailService.Verify(e => e.SendDelegationRequestRefusedEmailAsync(It.Is<Contact>(c => c.ContactId == 7), It.Is<AccountDetail>(a => a.AccountId == 200), It.IsAny<IEnumerable<int>>()), Times.Once);
        _mockDelegationRequestEventPublisher.Verify(p => p.PublishDelegationRequestRefusedEventAsync(currentUserId, 5, 100, null), Times.Once);
        _mockDelegationRequestEventPublisher.Verify(p => p.PublishDelegationRequestRefusedEventAsync(currentUserId, 7, 200, null), Times.Once);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WhenSameRecipientRefusedSeveralTimes_ShouldSendRefuserOnce()
    {
        var currentUserId = 10;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };

        SetupRefuseScenario(currentUserId, request, allSiblingsRefused: true);
        _mockRepository.Setup(r => r.GetRefusedSiblingRequestsAsync(5, 100)).ReturnsAsync(new List<DelegationRequest>
        {
            new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "refused" },
            new DelegationRequest { DelegationRequestId = 9, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "refused" }
        });

        await _service.RefuseRequestsAsync(currentUserId, request);

        _mockEmailService.Verify(
            e => e.SendDelegationRequestRefusedEmailAsync(It.Is<Contact>(c => c.ContactId == 5), It.Is<AccountDetail>(a => a.AccountId == 100), It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 10 }))),
            Times.Once);
    }

    private void SetupRefuseScenario(int currentUserId, RefuseDelegationRequestsRequest request, bool allSiblingsRefused)
    {
        var account = new Core.Models.Account { AccountId = 100, AccountNumber = "ACC100", AccountType = "REGULAR" };
        var refusedSiblingRequests = new List<DelegationRequest>
        {
            new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "refused", Account = account },
            new DelegationRequest { DelegationRequestId = 2, RecipientId = 11, RequesterId = 5, AccountId = 100, Status = "refused", Account = account }
        };

        _mockRepository.Setup(r => r.GetPendingRequestsByIdsAndRecipientAsync(request.DelegationRequestIds, currentUserId))
            .ReturnsAsync(new List<DelegationRequest>
            {
                new DelegationRequest { DelegationRequestId = 1, RecipientId = 10, RequesterId = 5, AccountId = 100, Status = "pending" }
            });
        _mockRepository.Setup(r => r.RefuseRequestsAsync(It.IsAny<int[]>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.AreAllSiblingRequestsRefusedAsync(5, 100)).ReturnsAsync(allSiblingsRefused);
        _mockRepository.Setup(r => r.GetRefusedSiblingRequestsAsync(5, 100)).ReturnsAsync(refusedSiblingRequests);
        _mockAccountRepository
            .Setup(r => r.GetAccountAsync(100))
            .ReturnsAsync(new AccountDetail { AccountId = 100, AccountNumber = "ACC100", AccountType = "REGULAR", Legal = new Legal { LegalName = "Legal100" }, Phone = new List<Phone>() });
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenValidRecipients_ShouldSendRequestEmailForEachRecipient()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        SetupValidCreationScenario(contactId, request, validRecipientIds: new[] { 2, 3 });

        await _service.CreateDelegationRequestsAsync(contactId, request);

        _mockEmailService.Verify(
            e => e.SendDelegationRequestEmailsAsync(
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 2, 3 })),
                It.Is<Contact>(c => c.ContactId == contactId),
                It.Is<AccountDetail>(a => a.AccountId == request.AccountId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenNoValidRecipient_ShouldThrowAndNotSendEmail()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);

        // Recipient has no role on account => invalid recipient
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(2, request.AccountId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.RecipientDoesNotHaveAccessCode);
        _mockEmailService.Verify(e => e.SendDelegationRequestEmailsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<Contact>(), It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenValidRecipients_ShouldPublishDelegationRequestCreatedEvent()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        SetupValidCreationScenario(contactId, request, validRecipientIds: new[] { 2, 3 });

        await _service.CreateDelegationRequestsAsync(contactId, request);

        _mockDelegationRequestEventPublisher.Verify(
            p => p.PublishDelegationRequestCreatedEventAsync(
                It.Is<AccountDetail>(a => a.AccountId == request.AccountId),
                contactId,
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 2, 3 }))),
            Times.Once);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenSomeRecipientsInvalid_ShouldPublishEventWithOnlyValidRecipients()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);

        // Recipient 2 is valid
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(2, request.AccountId)).ReturnsAsync(true);

        // Recipient 3 is invalid (no role on account)
        _mockRepository.Setup(r => r.DoesContactExistAsync(3)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(3, request.AccountId)).ReturnsAsync(false);

        _mockRepository
            .Setup(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.IsAny<int[]>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _service.CreateDelegationRequestsAsync(contactId, request);

        _mockDelegationRequestEventPublisher.Verify(
            p => p.PublishDelegationRequestCreatedEventAsync(
                It.Is<AccountDetail>(a => a.AccountId == request.AccountId),
                contactId,
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 2 }))),
            Times.Once);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenNoValidRecipient_ShouldNotPublishEvent()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(2, request.AccountId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .Where(ex => ex.Code == Errors.RecipientDoesNotHaveAccessCode);
        _mockDelegationRequestEventPublisher.Verify(
            p => p.PublishDelegationRequestCreatedEventAsync(It.IsAny<AccountDetail>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    private void SetupValidCreationScenario(int contactId, CreateDelegationRequestsRequest request, int[] validRecipientIds)
    {
        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRoleOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasActiveDelegationOnAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);

        foreach (var recipientId in validRecipientIds)
        {
            _mockRepository.Setup(r => r.DoesContactExistAsync(recipientId)).ReturnsAsync(true);
            _mockRepository.Setup(r => r.HasRoleOnAccountAsync(recipientId, request.AccountId)).ReturnsAsync(true);
        }

        _mockRepository
            .Setup(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, It.IsAny<int[]>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }
}
