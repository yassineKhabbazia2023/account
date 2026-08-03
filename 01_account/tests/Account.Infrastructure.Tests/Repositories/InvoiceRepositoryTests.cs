// <copyright file="InvoiceRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class InvoiceRepositoryTests
{
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
        var result = await repository.ExistsByInvoiceNumberAsync("INV-001");

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
            DocumentPath = "/path/to/test-invoice.pdf",
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
        Assert.Equal("/path/to/test-invoice.pdf", savedInvoice.DocumentPath);
        Assert.Equal(5, savedInvoice.AccountId);
        Assert.Equal(new DateTime(2024, 01, 15), savedInvoice.InvoiceDate);
        Assert.Equal(new DateTime(2024, 01, 16), savedInvoice.DepositDate);
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
            DocumentPath = "/path/to/test.pdf",
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
        await repository.RemoveByInvoiceNumberAsync("INV-001");

        // Assert
        var deletedInvoice = await context.InvoiceEntity
            .FirstOrDefaultAsync(i => i.InvoiceNumber == "INV-001");

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
            DocumentPath = "/path/to/test.pdf",
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
        var result = await repository.GetByInvoiceNumberAsync("INV-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-001", result.InvoiceNumber);
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
        var account = new AccountEntity
        {
            AccountNumber = "ACC123",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "unit-test",
            LegalName = "Account Inactive",
            CreationDate = DateTime.UtcNow,
            IsActive = false,
            AccountType = "COMPANY"
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new InvoiceRepository(context);

        // Act
        var result = await repository.GetAccountIdByAccountNumberAsync("ACC123");

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
}
