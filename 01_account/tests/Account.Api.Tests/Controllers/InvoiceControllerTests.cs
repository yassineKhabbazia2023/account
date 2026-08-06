// <copyright file="InvoiceControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Account.Api.Tests.Controllers;

public class InvoiceControllerTests
{
    private readonly Mock<IInvoiceService> _mockInvoiceService;
    private readonly InvoiceController _controller;

    public InvoiceControllerTests()
    {
        _mockInvoiceService = new Mock<IInvoiceService>();
        _controller = new InvoiceController(_mockInvoiceService.Object);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldReturnOkWithInvoices_WhenInvoicesAreRetrieved()
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria { Search = "INV" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var fixture = new Fixture();
        var expectedInvoices = fixture.Build<Invoice>().CreateMany(5).ToList();
        var paging = new Paging<Invoice>
        {
            CurrentPage = 1,
            Items = expectedInvoices,
            TotalItems = 5,
            TotalPage = 1
        };
        _mockInvoiceService
            .Setup(s => s.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination?>()))
            .ReturnsAsync(paging);

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPaging = okResult.Value.Should().BeAssignableTo<Paging<Invoice>>().Subject;
        returnedPaging.Items.Should().BeEquivalentTo(expectedInvoices);
        returnedPaging.TotalItems.Should().Be(5);
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(accountId, criteria, pagination), Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldReturnNoContent_WhenNoInvoicesFound()
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria();
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var paging = new Paging<Invoice>
        {
            CurrentPage = 1,
            Items = [],
            TotalItems = 0,
            TotalPage = 0
        };
        _mockInvoiceService
            .Setup(s => s.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination?>()))
            .ReturnsAsync(paging);

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(accountId, criteria, pagination), Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldReturnOk_WhenPaginationIsNull()
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria();
        Pagination? pagination = null;
        var fixture = new Fixture();
        var expectedInvoices = fixture.Build<Invoice>().CreateMany(3).ToList();
        var paging = new Paging<Invoice>
        {
            CurrentPage = 1,
            Items = expectedInvoices,
            TotalItems = 3,
            TotalPage = 1
        };
        _mockInvoiceService
            .Setup(s => s.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), null))
            .ReturnsAsync(paging);

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPaging = okResult.Value.Should().BeAssignableTo<Paging<Invoice>>().Subject;
        returnedPaging.TotalItems.Should().Be(3);
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(accountId, criteria, null), Times.Once);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var accountId = 42;
        var criteria = new SearchInvoicesCriteria
        {
            Search = "test-search",
            SortBy = "depositDate",
            SortOrder = "desc"
        };
        var pagination = new Pagination { PageNumber = 2, PageSize = 20 };
        var paging = new Paging<Invoice>
        {
            CurrentPage = 2,
            Items = [],
            TotalItems = 0,
            TotalPage = 0
        };
        _mockInvoiceService
            .Setup(s => s.GetInvoicesAsync(accountId, criteria, pagination))
            .ReturnsAsync(paging);

        // Act
        await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        _mockInvoiceService.Verify(
            s => s.GetInvoicesAsync(
                It.Is<int>(id => id == 42),
                It.Is<SearchInvoicesCriteria>(c => c.Search == "test-search" && c.SortBy == "depositDate" && c.SortOrder == "desc"),
                It.Is<Pagination?>(p => p != null && p.PageNumber == 2 && p.PageSize == 20)),
            Times.Once);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("date")]
    [InlineData("")]
    [InlineData("Name1")]
    public async Task GetInvoicesAsync_ShouldReturnBadRequest_WhenSortByIsInvalid(string invalidSortBy)
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria { SortBy = invalidSortBy };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value!.ToString().Should().Contain("sortBy");
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(It.IsAny<int>(), It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination?>()), Times.Never);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("depositDate")]
    [InlineData("invoiceYear")]
    [InlineData("NAME")]
    [InlineData("DepositDate")]
    public async Task GetInvoicesAsync_ShouldNotReturnBadRequest_WhenSortByIsValid(string validSortBy)
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria { SortBy = validSortBy, SortOrder = "asc" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var paging = new Paging<Invoice>
        {
            CurrentPage = 1,
            Items = [],
            TotalItems = 0,
            TotalPage = 0
        };
        _mockInvoiceService
            .Setup(s => s.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination?>()))
            .ReturnsAsync(paging);

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        result.Result.Should().NotBeOfType<BadRequestObjectResult>();
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(accountId, criteria, pagination), Times.Once);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ascending")]
    [InlineData("")]
    [InlineData("ASC1")]
    public async Task GetInvoicesAsync_ShouldReturnBadRequest_WhenSortOrderIsInvalid(string invalidSortOrder)
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria { SortBy = "name", SortOrder = invalidSortOrder };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value!.ToString().Should().Contain("sortOrder");
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(It.IsAny<int>(), It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination?>()), Times.Never);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    [InlineData("ASC")]
    [InlineData("DESC")]
    public async Task GetInvoicesAsync_ShouldNotReturnBadRequest_WhenSortOrderIsValid(string validSortOrder)
    {
        // Arrange
        var accountId = 1;
        var criteria = new SearchInvoicesCriteria { SortBy = "name", SortOrder = validSortOrder };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var paging = new Paging<Invoice>
        {
            CurrentPage = 1,
            Items = [],
            TotalItems = 0,
            TotalPage = 0
        };
        _mockInvoiceService
            .Setup(s => s.GetInvoicesAsync(accountId, It.IsAny<SearchInvoicesCriteria>(), It.IsAny<Pagination?>()))
            .ReturnsAsync(paging);

        // Act
        var result = await _controller.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        result.Result.Should().NotBeOfType<BadRequestObjectResult>();
        _mockInvoiceService.Verify(s => s.GetInvoicesAsync(accountId, criteria, pagination), Times.Once);
    }
}
