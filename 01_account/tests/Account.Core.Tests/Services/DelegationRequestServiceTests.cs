// <copyright file="DelegationRequestServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

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

namespace Pulse.Account.Core.Tests.Services;

public class DelegationRequestServiceTests
{
    private readonly Mock<IDelegationRequestRepository> _mockRepository;
    private readonly DelegationRequestService _service;

    public DelegationRequestServiceTests()
    {
        _mockRepository = new Mock<IDelegationRequestRepository>();
        _service = new DelegationRequestService(_mockRepository.Object);
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
        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRecipientAccessToAccountAsync(2, request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRecipientAccessToAccountAsync(3, request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, request.AccountId)).ReturnsAsync(false);

        await _service.CreateDelegationRequestsAsync(contactId, request);

        _mockRepository.Verify(r => r.CreateDelegationRequestsAsync(contactId, request.AccountId, request.RecipientIds, It.IsAny<string>()), Times.Once);
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
    public async Task CreateDelegationRequestsAsync_WhenRequesterDoesNotExist_ShouldThrowNotFoundException()
    {
        var contactId = 1;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _mockRepository.Setup(r => r.DoesAccountExistAsync(request.AccountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DoesContactExistAsync(contactId)).ReturnsAsync(false);

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Code == Errors.NotFoundContactCode);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRecipientDoesNotExist_ShouldThrowNotFoundException()
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

        Func<Task> act = async () => await _service.CreateDelegationRequestsAsync(contactId, request);

        await act.Should().ThrowAsync<NotFoundException>()
            .Where(ex => ex.Code == Errors.NotFoundContactsCode);
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
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, request.AccountId)).ReturnsAsync(true);

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
        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRecipientAccessToAccountAsync(2, request.AccountId)).ReturnsAsync(false);

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
        _mockRepository.Setup(r => r.DoesContactExistAsync(2)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, request.AccountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasRecipientAccessToAccountAsync(2, request.AccountId)).ReturnsAsync(true);
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

        _mockRepository.Setup(r => r.GetReceivedRequestsAsync(contactId, "pending", It.IsAny<Pagination>())).ReturnsAsync(expectedResult);

        var result = await _service.GetReceivedRequestsAsync(contactId, pagination);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetReceivedRequestsAsync(contactId, "pending", It.IsAny<Pagination>()), Times.Once);
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

        _mockRepository.Setup(r => r.GetReceivedRequestsAsync(contactId, "pending", It.IsAny<Pagination>())).ReturnsAsync(expectedResult);

        var result = await _service.GetReceivedRequestsAsync(contactId, null);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetReceivedRequestsAsync(contactId, "pending", It.Is<Pagination>(p => p.PageNumber == 1 && p.PageSize == int.MaxValue)), Times.Once);
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

        _mockRepository.Setup(r => r.GetReceivedRequestsAsync(contactId, "pending", It.Is<Pagination>(p => p.PageNumber == expectedPageNumber && p.PageSize == expectedPageSize))).ReturnsAsync(expectedResult);

        var result = await _service.GetReceivedRequestsAsync(contactId, pagination);

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetReceivedRequestsAsync(contactId, "pending", It.Is<Pagination>(p => p.PageNumber == expectedPageNumber && p.PageSize == expectedPageSize)), Times.Once);
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenUserHasAccess_ShouldReturnAlreadyInPortfolio()
    {
        var contactId = 1;
        var accountId = 100;

        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, accountId)).ReturnsAsync(true);

        var result = await _service.CheckEligibilityAsync(contactId, accountId);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Be(DelegationEligibilityReason.AlreadyInPortfolio);
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenPendingRequestExists_ShouldReturnAlreadyPending()
    {
        var contactId = 1;
        var accountId = 100;

        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, accountId)).ReturnsAsync(false);
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

        _mockRepository.Setup(r => r.HasRequesterAccessToAccountAsync(contactId, accountId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.HasPendingRequestAsync(contactId, accountId)).ReturnsAsync(false);

        var result = await _service.CheckEligibilityAsync(contactId, accountId);

        result.IsEligible.Should().BeTrue();
        result.Reason.Should().BeNull();
    }
}
