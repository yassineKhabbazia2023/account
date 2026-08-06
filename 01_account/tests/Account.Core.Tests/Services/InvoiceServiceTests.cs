// <copyright file="InvoiceServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepository;
    private readonly Mock<IDelegationRequestRepository> _mockDelegationRequestRepository;
    private readonly InvoiceService _invoiceService;
    private readonly Fixture _fixture;

    public InvoiceServiceTests()
    {
        _mockInvoiceRepository = new Mock<IInvoiceRepository>();
        _mockDelegationRequestRepository = new Mock<IDelegationRequestRepository>();
        _invoiceService = new InvoiceService(_mockInvoiceRepository.Object, _mockDelegationRequestRepository.Object);
        _fixture = new Fixture();

        _mockDelegationRequestRepository
            .Setup(repo => repo.DoesAccountExistAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task GetInvoicesAsync_ReturnsInvoicesFromRepository()
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Build<SearchInvoicesCriteria>()
            .With(c => c.SortBy, "Name")
            .With(c => c.SortOrder, "asc")
            .Create();
        var pagination = _fixture.Build<Pagination>()
            .With(p => p.PageNumber, 1)
            .With(p => p.PageSize, 10)
            .Create();

        var expectedInvoices = _fixture.CreateMany<Invoice>(5).ToList();
        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, expectedInvoices)
            .With(p => p.CurrentPage, 1)
            .With(p => p.TotalPage, 1)
            .With(p => p.TotalItems, expectedInvoices.Count)
            .Create();

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.CurrentPage, result.CurrentPage);
        Assert.Equal(expectedResult.TotalPage, result.TotalPage);
        Assert.Equal(expectedResult.TotalItems, result.TotalItems);
        Assert.Equal(expectedResult.Items.Count(), result.Items.Count());

        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()),
            Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenPaginationIsNull_ShouldUseDefaultPagination()
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Create<SearchInvoicesCriteria>();
        Pagination? pagination = null;

        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, Enumerable.Empty<Invoice>())
            .With(p => p.TotalItems, 0)
            .Create();

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(
                accountId,
                It.IsAny<SearchInvoicesCriteria>(),
                It.Is<Pagination>(p => p.PageNumber == 1 && p.PageSize == int.MaxValue)),
            Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenPageNumberIsZeroOrNegative_ShouldUsePageOne()
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Create<SearchInvoicesCriteria>();
        var pagination = _fixture.Build<Pagination>()
            .With(p => p.PageNumber, 0)
            .With(p => p.PageSize, 10)
            .Create();

        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, Enumerable.Empty<Invoice>())
            .With(p => p.TotalItems, 0)
            .Create();

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(
                accountId,
                It.IsAny<SearchInvoicesCriteria>(),
                It.Is<Pagination>(p => p.PageNumber == 1)),
            Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenPageSizeIsZeroOrNegative_ShouldUseMaxValue()
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Create<SearchInvoicesCriteria>();
        var pagination = _fixture.Build<Pagination>()
            .With(p => p.PageNumber, 1)
            .With(p => p.PageSize, 0)
            .Create();

        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, Enumerable.Empty<Invoice>())
            .With(p => p.TotalItems, 0)
            .Create();

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(
                accountId,
                It.IsAny<SearchInvoicesCriteria>(),
                It.Is<Pagination>(p => p.PageSize == int.MaxValue)),
            Times.Once);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("DepositDate")]
    [InlineData("InvoiceYear")]
    public async Task GetInvoicesAsync_WhenSortByIsValid_ShouldUseThatSortField(string sortBy)
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Build<SearchInvoicesCriteria>()
            .With(c => c.SortBy, sortBy)
            .With(c => c.SortOrder, "asc")
            .Create();
        var pagination = _fixture.Create<Pagination>();

        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, Enumerable.Empty<Invoice>())
            .With(p => p.TotalItems, 0)
            .Create();

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(
                accountId,
                It.Is<SearchInvoicesCriteria>(c => c.SortBy == sortBy),
                It.IsAny<Pagination>()),
            Times.Once);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task GetInvoicesAsync_WhenSortOrderIsValid_ShouldUseThatSortOrder(string sortOrder)
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Build<SearchInvoicesCriteria>()
            .With(c => c.SortBy, "Name")
            .With(c => c.SortOrder, sortOrder)
            .Create();
        var pagination = _fixture.Create<Pagination>();

        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, Enumerable.Empty<Invoice>())
            .With(p => p.TotalItems, 0)
            .Create();

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(
                accountId,
                It.Is<SearchInvoicesCriteria>(c => c.SortOrder == sortOrder),
                It.IsAny<Pagination>()),
            Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenAccountDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Build<SearchInvoicesCriteria>()
            .With(c => c.SortBy, "Name")
            .With(c => c.SortOrder, "asc")
            .Create();
        var pagination = _fixture.Create<Pagination>();

        _mockDelegationRequestRepository
            .Setup(repo => repo.DoesAccountExistAsync(accountId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _invoiceService.GetInvoicesAsync(accountId, criteria, pagination));

        Assert.Equal(Errors.NotFoundAccountCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundAccountMessage, accountId), exception.Message);

        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(It.IsAny<int>(), It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()),
            Times.Never);
    }

    [Fact]
    public async Task GetInvoicesAsync_WhenAccountExists_ShouldNotThrowNotFoundException()
    {
        // Arrange
        var accountId = _fixture.Create<int>();
        var criteria = _fixture.Build<SearchInvoicesCriteria>()
            .With(c => c.SortBy, "Name")
            .With(c => c.SortOrder, "asc")
            .Create();
        var pagination = _fixture.Create<Pagination>();

        var expectedResult = _fixture.Build<Paging<Invoice>>()
            .With(p => p.Items, Enumerable.Empty<Invoice>())
            .With(p => p.TotalItems, 0)
            .Create();

        _mockDelegationRequestRepository
            .Setup(repo => repo.DoesAccountExistAsync(accountId))
            .ReturnsAsync(true);

        _mockInvoiceRepository
            .Setup(repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _invoiceService.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        _mockDelegationRequestRepository.Verify(
            repo => repo.DoesAccountExistAsync(accountId),
            Times.Once);
        _mockInvoiceRepository.Verify(
            repo => repo.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination>()),
            Times.Once);
    }
}
