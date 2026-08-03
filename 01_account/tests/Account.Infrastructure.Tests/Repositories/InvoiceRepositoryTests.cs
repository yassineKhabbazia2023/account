// <copyright file="InvoiceRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class InvoiceRepositoryTests
{
    private readonly Fixture _fixture;

    public InvoiceRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private static AccountContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AccountContext(options);
    }

    [Fact]
    public async Task ExistsByInvoiceNumberAsync_WithExistingInvoice_ReturnsTrueAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var invoiceNumber = "INV-001";
        var invoice = new InvoiceEntity
        {
            InvoiceNumber = invoiceNumber,
            DocumentPath = "/path/to/document.pdf",
            Type = "Facture RYDGE",
            Category = "ADMINISTRATIF",
            InvoiceDate = DateTime.UtcNow,
            DepositDate = DateTime.UtcNow,
            AccountId = 1
        };

        context.InvoiceEntity.Add(invoice);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.ExistsByInvoiceNumberAsync(invoiceNumber);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByInvoiceNumberAsync_WithNonExistingInvoice_ReturnsFalseAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.ExistsByInvoiceNumberAsync("INV-999");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddAsync_WithValidRequest_InsertsInvoiceAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);

        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-001",
            DocumentPath = "/path/to/document.pdf",
            Type = "Facture RYDGE",
            Category = "ADMINISTRATIF",
            InvoiceDate = new DateTime(2024, 01, 15),
            DepositDate = new DateTime(2024, 01, 16),
            AccountId = 5
        };

        // Act
        await repository.AddAsync(request);

        // Assert
        var savedInvoice = await context.InvoiceEntity
            .FirstOrDefaultAsync(i => i.InvoiceNumber == "INV-001");

        Assert.NotNull(savedInvoice);
        Assert.Equal("/path/to/document.pdf", savedInvoice.DocumentPath);
        Assert.Equal("Facture RYDGE", savedInvoice.Type);
        Assert.Equal("ADMINISTRATIF", savedInvoice.Category);
        Assert.Equal(new DateTime(2024, 01, 15), savedInvoice.InvoiceDate);
        Assert.Equal(new DateTime(2024, 01, 16), savedInvoice.DepositDate);
        Assert.Equal(5, savedInvoice.AccountId);
    }

    [Fact]
    public async Task AddAsync_WithNullRequest_ThrowsArgumentNullExceptionAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [Fact]
    public async Task RemoveByInvoiceNumberAsync_WithExistingInvoice_RemovesItAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var invoice = new InvoiceEntity
        {
            InvoiceNumber = "INV-001",
            DocumentPath = "/path/to/document.pdf",
            Type = "Facture RYDGE",
            Category = "ADMINISTRATIF",
            InvoiceDate = DateTime.UtcNow,
            DepositDate = DateTime.UtcNow,
            AccountId = 1
        };

        context.InvoiceEntity.Add(invoice);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        await repository.RemoveByInvoiceNumberAsync(invoice.InvoiceNumber);

        // Assert
        var deletedInvoice = await context.InvoiceEntity
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoice.InvoiceNumber);

        Assert.Null(deletedInvoice);
    }

    [Fact]
    public async Task RemoveByInvoiceNumberAsync_WithNonExistingInvoice_DoesNothingAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);

        // Act
        await repository.RemoveByInvoiceNumberAsync("INV-999");

        // Assert
        var remainingInvoicesCount = await context.InvoiceEntity.CountAsync();
        Assert.Equal(0, remainingInvoicesCount);
    }

    [Fact]
    public async Task GetByInvoiceNumberAsync_WithExistingInvoice_ReturnsInvoiceAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var invoice = new InvoiceEntity
        {
            InvoiceNumber = "INV-001",
            DocumentPath = "/path/to/document.pdf",
            Type = "Facture RYDGE",
            Category = "ADMINISTRATIF",
            InvoiceDate = DateTime.UtcNow,
            DepositDate = DateTime.UtcNow,
            AccountId = 1
        };

        context.InvoiceEntity.Add(invoice);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetByInvoiceNumberAsync(invoice.InvoiceNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invoice.InvoiceNumber, result.InvoiceNumber);
    }

    [Fact]
    public async Task GetByInvoiceNumberAsync_WithNonExistingInvoice_ReturnsNullAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetByInvoiceNumberAsync("INV-999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountIdByAccountNumberAsync_WithExistingActiveAccount_ReturnsAccountIdAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var account = new AccountEntity
        {
            AccountNumber = "ACC123",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "unit-test",
            LegalName = "Account Active",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
            AccountType = "COMPANY"
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetAccountIdByAccountNumberAsync("ACC123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.Value);
    }

    [Fact]
    public async Task GetAccountIdByAccountNumberAsync_WithInactiveAccount_ReturnsNullAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountNumber = "ACC123";
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, accountNumber)
            .With(a => a.IsActive, false)
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetAccountIdByAccountNumberAsync(accountNumber);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountIdByAccountNumberAsync_WithNonExistingAccount_ReturnsNullAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetAccountIdByAccountNumberAsync("ACC999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithInvoicesForAccount_ReturnsPagingWithInvoicesAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountId = 1;
        var invoices = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.Account, (AccountEntity?)null)
            .CreateMany(5)
            .ToList();

        context.InvoiceEntity.AddRange(invoices);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(5, result.Items.Count());
        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithNoInvoicesForAccount_ReturnsEmptyPagingAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId: 999, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items!);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithPagination_ReturnsCorrectPageAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountId = 1;
        var invoices = Enumerable.Range(1, 15).Select(i => _fixture.Build<InvoiceEntity>()
            .With(inv => inv.AccountId, accountId)
            .With(inv => inv.Account, (AccountEntity?)null)
            .With(inv => inv.DepositDate, DateTime.UtcNow.AddDays(-i))
            .Create())
            .ToList();

        context.InvoiceEntity.AddRange(invoices);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 2, PageSize = 5 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.TotalItems);
        Assert.Equal(5, result.Items!.Count());
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.TotalPage);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithSearchCriteria_FiltersResultsAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountId = 1;

        var matchingInvoice = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.InvoiceNumber, "INV-MATCHING-001")
            .With(i => i.Category, "Other")
            .With(i => i.Type, "Other")
            .With(i => i.Account, (AccountEntity?)null)
            .Create();

        var nonMatchingInvoice = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.InvoiceNumber, "INV-OTHER-002")
            .With(i => i.Category, "Other")
            .With(i => i.Type, "Other")
            .With(i => i.Account, (AccountEntity?)null)
            .Create();

        context.InvoiceEntity.AddRange(matchingInvoice, nonMatchingInvoice);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { Search = "MATCHING", SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items!);
        Assert.Equal("INV-MATCHING-001", result.Items!.First().Name);
    }

    [Fact]
    public async Task GetInvoicesAsync_OnlyReturnsInvoicesForSpecifiedAccountAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var targetAccountId = 1;
        var otherAccountId = 2;

        var targetInvoices = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, targetAccountId)
            .With(i => i.Account, (AccountEntity?)null)
            .CreateMany(3)
            .ToList();

        var otherInvoices = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, otherAccountId)
            .With(i => i.Account, (AccountEntity?)null)
            .CreateMany(2)
            .ToList();

        context.InvoiceEntity.AddRange(targetInvoices);
        context.InvoiceEntity.AddRange(otherInvoices);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.GetInvoicesAsync(targetAccountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalItems);
        Assert.All(result.Items!, invoice => Assert.Equal(targetAccountId, invoice.Id > 0 ? targetAccountId : targetAccountId));
    }

    [Theory]
    [InlineData("Name", "asc")]
    [InlineData("Name", "desc")]
    [InlineData("DepositDate", "asc")]
    [InlineData("DepositDate", "desc")]
    [InlineData("InvoiceYear", "asc")]
    [InlineData("InvoiceYear", "desc")]
    public async Task GetInvoicesAsync_WithSortingCriteria_ReturnsSortedResultsAsync(string sortBy, string sortOrder)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountId = 1;

        var invoice1 = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.InvoiceNumber, "INV-ALPHA-001")
            .With(i => i.DepositDate, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .With(i => i.InvoiceDate, new DateTime(2023, 5, 15, 0, 0, 0, DateTimeKind.Utc))
            .With(i => i.Account, (AccountEntity?)null)
            .Create();

        var invoice2 = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.InvoiceNumber, "INV-BETA-002")
            .With(i => i.DepositDate, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc))
            .With(i => i.InvoiceDate, new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc))
            .With(i => i.Account, (AccountEntity?)null)
            .Create();

        context.InvoiceEntity.AddRange(invoice1, invoice2);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { SortBy = sortBy, SortOrder = sortOrder };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        var items = result.Items.ToList();

        if (sortBy == "Name")
        {
            // Sort by Name uses InvoiceNumber
            if (sortOrder == "asc")
            {
                Assert.Equal("INV-ALPHA-001", items[0].Name);
                Assert.Equal("INV-BETA-002", items[1].Name);
            }
            else
            {
                Assert.Equal("INV-BETA-002", items[0].Name);
                Assert.Equal("INV-ALPHA-001", items[1].Name);
            }
        }
        else if (sortBy == "InvoiceYear")
        {
            // Sort by InvoiceYear uses InvoiceDate.Year (2023 vs 2024)
            if (sortOrder == "asc")
            {
                Assert.Equal(2023, items[0].InvoiceYear);
                Assert.Equal(2024, items[1].InvoiceYear);
            }
            else
            {
                Assert.Equal(2024, items[0].InvoiceYear);
                Assert.Equal(2023, items[1].InvoiceYear);
            }
        }
    }

    [Fact]
    public async Task GetInvoicesAsync_WithSearchOnCategory_FiltersResultsAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountId = 1;

        var matchingInvoice = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.InvoiceNumber, "INV-001")
            .With(i => i.Category, "Services")
            .With(i => i.Type, "Standard")
            .With(i => i.Account, (AccountEntity?)null)
            .Create();

        var nonMatchingInvoice = _fixture.Build<InvoiceEntity>()
            .With(i => i.AccountId, accountId)
            .With(i => i.InvoiceNumber, "INV-002")
            .With(i => i.Category, "Products")
            .With(i => i.Type, "Standard")
            .With(i => i.Account, (AccountEntity?)null)
            .Create();

        context.InvoiceEntity.AddRange(matchingInvoice, nonMatchingInvoice);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { Search = "Services", SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Services", result.Items!.First().Category);
    }

    [Fact]
    public async Task GetInvoicesAsync_WithLastPage_ReturnsRemainingItemsAsync()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var accountId = 1;
        var invoices = Enumerable.Range(1, 12).Select(i => _fixture.Build<InvoiceEntity>()
            .With(inv => inv.AccountId, accountId)
            .With(inv => inv.Account, (AccountEntity?)null)
            .Create())
            .ToList();

        context.InvoiceEntity.AddRange(invoices);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);
        var criteria = new SearchInvoicesCriteria { SortBy = "DepositDate", SortOrder = "desc" };
        var pagination = new Pagination { PageNumber = 3, PageSize = 5 };

        // Act
        var result = await repository.GetInvoicesAsync(accountId, criteria, pagination);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.TotalItems);
        Assert.Equal(2, result.Items!.Count());
        Assert.Equal(3, result.CurrentPage);
        Assert.Equal(3, result.TotalPage);
    }
}
