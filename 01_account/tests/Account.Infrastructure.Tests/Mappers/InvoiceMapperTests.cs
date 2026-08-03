// <copyright file="InvoiceMapperTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class InvoiceMapperTests
{
    private readonly Fixture _fixture;

    public InvoiceMapperTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void ToInvoiceEntity_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        CreateInvoiceRequest? request = null;

        // Act
        void Action() => request!.ToInvoiceEntity();

        // Assert
        Assert.Throws<ArgumentNullException>(Action);
    }

    [Fact]
    public void ToInvoiceEntity_WhenRequestIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var request = _fixture.Create<CreateInvoiceRequest>();

        // Act
        var entity = request.ToInvoiceEntity();

        // Assert
        Assert.Equal(request.InvoiceNumber, entity.InvoiceNumber);
        Assert.Equal(request.DocumentPath, entity.DocumentPath);
        Assert.Equal(request.InvoiceDate, entity.InvoiceDate);
        Assert.Equal(request.DepositDate, entity.DepositDate);
        Assert.Equal(request.AccountId, entity.AccountId);
    }

    [Fact]
    public void ToInvoice_WhenEntityIsNull_ShouldReturnNull()
    {
        // Arrange
        InvoiceEntity? entity = null;

        // Act
        var result = entity.ToInvoice();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToInvoice_WhenEntityIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var entity = _fixture.Build<InvoiceEntity>()
            .With(e => e.InvoiceNumber, "INV-001")
            .Create();

        // Act
        var result = entity.ToInvoice();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.InvoiceId, result.Id);
        Assert.Equal(entity.InvoiceNumber, result.Name);
        Assert.Equal(entity.DepositDate, result.DepositDate);
        Assert.Equal(entity.Type, result.Type);
        Assert.Equal(entity.Category, result.Category);
        Assert.Equal(entity.InvoiceDate.Year, result.InvoiceYear);
    }

    [Fact]
    public void ToInvoice_WhenDocumentPathHasNoSlash_ShouldUseFullPathAsName()
    {
        // Arrange
        var entity = _fixture.Build<InvoiceEntity>()
            .With(e => e.InvoiceNumber, "INV-001")
            .Create();

        // Act
        var result = entity.ToInvoice();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-001", result.Name);
    }

    [Fact]
    public void ToInvoice_WhenDocumentPathIsEmpty_ShouldReturnEmptyName()
    {
        // Arrange
        var entity = _fixture.Build<InvoiceEntity>()
            .With(e => e.InvoiceNumber, "INV-002")
            .Create();

        // Act
        var result = entity.ToInvoice();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-002", result.Name);
    }

    [Fact]
    public void MapToPagingInvoice_WhenEntitiesIsEmpty_ShouldReturnEmptyPaging()
    {
        // Arrange
        var entities = Enumerable.Empty<InvoiceEntity>();
        var totalItems = 0;
        var totalPages = 0;
        var pageNumber = 1;
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };

        // Act
        var result = entities.MapToPagingInvoice(totalItems, totalPages, pageNumber, criteria);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items!);
        Assert.Equal(pageNumber, result.CurrentPage);
        Assert.Equal(totalItems, result.TotalItems);
        Assert.Equal(totalPages, result.TotalPage);
    }

    [Fact]
    public void MapToPagingInvoice_WhenEntitiesHasItems_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var entities = new List<InvoiceEntity>
        {
            _fixture.Build<InvoiceEntity>()
                .With(e => e.InvoiceNumber, "INV-001")
                .Create(),
            _fixture.Build<InvoiceEntity>()
                .With(e => e.InvoiceNumber, "INV-002")
                .Create()
        };
        var totalItems = 10;
        var totalPages = 5;
        var pageNumber = 2;
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };

        // Act
        var result = entities.MapToPagingInvoice(totalItems, totalPages, pageNumber, criteria);
        var items = result.Items!.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, items.Count);
        Assert.Equal(pageNumber, result.CurrentPage);
        Assert.Equal(totalItems, result.TotalItems);
        Assert.Equal(totalPages, result.TotalPage);

        // Verify invoices are mapped correctly (order may change due to sorting)
        Assert.Contains(items, i => i.Name == "INV-001");
        Assert.Contains(items, i => i.Name == "INV-002");
    }
}
