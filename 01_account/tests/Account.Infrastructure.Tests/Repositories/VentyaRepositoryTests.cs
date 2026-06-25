// <copyright file="VentyaRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class VentyaRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public VentyaRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static DbContextOptions<AccountContext> CreateNewDbContextOptions()
    {
        return new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_WithValidCustomerAndFlag_ShouldReturnHasAccessTrue()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account = new AccountEntity
        {
            AccountNumber = "ACC001",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var contact = new ContactEntity
        {
            ContactId = 100,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.fr",
            Type = ContactType.Customer.ToString(),
            PersonaName = "Client",
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var role = new RoleEntity
        {
            AccountId = account.AccountId,
            ContactId = contact.ContactId,
            ContactFlagPortailFactures = true
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account.AccountId, 100);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeTrue();
        result.RoleFound.Should().BeTrue();
        result.HasAccess.Should().BeTrue();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_WithCustomerAndFlagFalse_ShouldReturnHasAccessFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account = new AccountEntity
        {
            AccountNumber = "ACC002",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 2",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var contact = new ContactEntity
        {
            ContactId = 200,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@test.fr",
            Type = ContactType.Customer.ToString(),
            PersonaName = "Client",
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var role = new RoleEntity
        {
            AccountId = account.AccountId,
            ContactId = contact.ContactId,
            ContactFlagPortailFactures = false
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account.AccountId, 200);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeTrue();
        result.RoleFound.Should().BeTrue();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_WithCustomerAndFlagNull_ShouldReturnHasAccessFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account = new AccountEntity
        {
            AccountNumber = "ACC003",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 3",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var contact = new ContactEntity
        {
            ContactId = 300,
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob.smith@test.fr",
            Type = ContactType.Customer.ToString(),
            PersonaName = "Client",
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var role = new RoleEntity
        {
            AccountId = account.AccountId,
            ContactId = contact.ContactId,
            ContactFlagPortailFactures = null
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account.AccountId, 300);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeTrue();
        result.RoleFound.Should().BeTrue();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_WithCollaborator_ShouldReturnHasAccessFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account = new AccountEntity
        {
            AccountNumber = "ACC004",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 4",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var contact = new ContactEntity
        {
            ContactId = 400,
            FirstName = "Alice",
            LastName = "Brown",
            Email = "alice.brown@test.fr",
            Type = ContactType.Collaborator.ToString(),
            PersonaName = "Collaborateur",
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var role = new RoleEntity
        {
            AccountId = account.AccountId,
            ContactId = contact.ContactId,
            ContactFlagPortailFactures = true
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account.AccountId, 400);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeTrue();
        result.RoleFound.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_AccountNotFound_ShouldReturnAccountFoundFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(999, 100);

        // Assert
        result.AccountFound.Should().BeFalse();
        result.ContactFound.Should().BeFalse();
        result.RoleFound.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_ContactNotFound_ShouldReturnContactFoundFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account = new AccountEntity
        {
            AccountNumber = "ACC005",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 5",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account.AccountId, 999);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeFalse();
        result.RoleFound.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_RoleNotFound_ShouldReturnRoleFoundFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account = new AccountEntity
        {
            AccountNumber = "ACC006",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 6",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var contact = new ContactEntity
        {
            ContactId = 600,
            FirstName = "Charlie",
            LastName = "Davis",
            Email = "charlie.davis@test.fr",
            Type = ContactType.Customer.ToString(),
            PersonaName = "Client",
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account.AccountId, 600);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeTrue();
        result.RoleFound.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_RoleExistsOnDifferentAccount_ShouldReturnRoleFoundFalse()
    {
        // Arrange
        var options = CreateNewDbContextOptions();
        using var context = new AccountContext(options);

        var account1 = new AccountEntity
        {
            AccountNumber = "ACC007",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 7",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account1);
        await context.SaveChangesAsync();

        var account2 = new AccountEntity
        {
            AccountNumber = "ACC008",
            AccountGlobalUniqueId = Guid.NewGuid(),
            CreatedBy = "UnitTest",
            LegalName = "Test Company 8",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        };
        context.AccountEntity.Add(account2);
        await context.SaveChangesAsync();

        var contact = new ContactEntity
        {
            ContactId = 700,
            FirstName = "David",
            LastName = "Wilson",
            Email = "david.wilson@test.fr",
            Type = ContactType.Customer.ToString(),
            PersonaName = "Client",
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var role = new RoleEntity
        {
            AccountId = account2.AccountId,
            ContactId = contact.ContactId,
            ContactFlagPortailFactures = true
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        // Act
        var result = await repository.CheckVentyaAccessAsync(account1.AccountId, 700);

        // Assert
        result.AccountFound.Should().BeTrue();
        result.ContactFound.Should().BeTrue();
        result.RoleFound.Should().BeFalse();
        result.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAccountEmailAsync_WhenAccountNotFound_ReturnsNotFound()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new VentyaRepository(context);

        var result = await repository.GetAccountEmailAsync(999);

        Assert.False(result.AccountExists);
        Assert.Null(result.AccountEmail);
    }

    [Fact]
    public async Task GetAccountEmailAsync_WhenAccountHasEmail_ReturnsEmail()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC123")
            .With(a => a.Email, "account@test.fr")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        var result = await repository.GetAccountEmailAsync(account.AccountId);

        Assert.NotNull(result);
        Assert.True(result.AccountExists);
        Assert.Equal("account@test.fr", result.AccountEmail);
    }

    [Fact]
    public async Task GetAccountEmailAsync_WhenAccountHasNoEmail_ReturnsAccountExistsWithNullEmail()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC124")
            .Without(a => a.Email)
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();
        account.Email = null;

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        var result = await repository.GetAccountEmailAsync(account.AccountId);

        Assert.NotNull(result);
        Assert.True(result.AccountExists);
        Assert.Null(result.AccountEmail);
    }

    [Fact]
    public async Task GetSsoContactIdAsync_WhenExactlyOneFlaggedContact_ReturnsContactId()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC125")
            .With(a => a.Email, "account@test.fr")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        account.RoleEntity = new List<RoleEntity>
        {
            new RoleEntity { AccountId = account.AccountId, ContactId = 10, ContactFlagPortailFactures = true },
            new RoleEntity { AccountId = account.AccountId, ContactId = 11, ContactFlagPortailFactures = false }
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        var result = await repository.GetSsoContactIdAsync(account.AccountId);

        Assert.Equal(10, result);
    }

    [Fact]
    public async Task GetSsoContactIdAsync_WhenMultipleFlaggedContacts_ReturnsNull()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC126")
            .With(a => a.Email, "account@test.fr")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        account.RoleEntity = new List<RoleEntity>
        {
            new RoleEntity { AccountId = account.AccountId, ContactId = 10, ContactFlagPortailFactures = true },
            new RoleEntity { AccountId = account.AccountId, ContactId = 11, ContactFlagPortailFactures = true }
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        var result = await repository.GetSsoContactIdAsync(account.AccountId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSsoContactIdAsync_WhenNoFlaggedContact_ReturnsNull()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC127")
            .With(a => a.Email, "account@test.fr")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        account.RoleEntity = new List<RoleEntity>
        {
            new RoleEntity { AccountId = account.AccountId, ContactId = 11, ContactFlagPortailFactures = false }
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new VentyaRepository(context);

        var result = await repository.GetSsoContactIdAsync(account.AccountId);

        Assert.Null(result);
    }
}
