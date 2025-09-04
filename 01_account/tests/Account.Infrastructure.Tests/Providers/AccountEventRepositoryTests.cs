// <copyright file="AccountEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class AccountEventRepositoryTests
{
    private readonly Fixture _fixture;

    public AccountEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetAccountQueryByContactId_WithContactId_ShouldReturnAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new AccountEventRepository(context);
        var contactEntity = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PersonaName = "Collab GS",
            Status = ContactStatus.Declared.ToString(),
            Type = "collaborator",
            CreationDate = DateTime.Parse("2024-04-16T09:19:16Z"),
            ContactGlobalUniqueId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF"),
        };

        var accountEntity = new AccountEntity
        {
            AccountId = 1,
            LegalName = "jhonny pizza",
            AccountNumber = "19999999",
            Email = "jhonny@test.com",
            CreatedBy = "test@test.com",
            IsActive = true
        };

        var roleEntity = new RoleEntity
        {
            AccountId = 1,
            ContactId = 1,
            IsDelegation = false,
            IsFavorite = false,
            IsSignatory = true
        };

        await context.ContactEntity.AddAsync(contactEntity);
        await context.AccountEntity.AddAsync(accountEntity);
        await context.RoleEntity.AddAsync(roleEntity);
        await context.SaveChangesAsync();

        // Act
        var accounts = repository.GetAccountBySignatory(contactEntity.ContactId);

        // Assert
        var accountByContact = accounts.FirstOrDefault();
        Assert.NotNull(accountByContact);
        Assert.Equal(accountEntity.AccountId, accountByContact.AccountId);
        Assert.Equal(accountEntity.LegalName, accountByContact.LegalName);
        Assert.Equal(accountEntity.AccountNumber, accountByContact.AccountNumber);
        Assert.Equal(accountEntity.Email, accountByContact.Email);
        Assert.Equal(accountEntity.CreatedBy, accountByContact.CreatedBy);
    }

    [Fact]
    public async Task UpdateAccountStatusByContactAsync_WithAccountIds_ShouldReturnUpdatedAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new AccountEventRepository(context);
        var deployment = _fixture.Build<DeploymentEntity>()
            .With(d => d.Status, 1)
            .With(d => d.AccountId, 1)
            .Create();

        await context.DeploymentEntity.AddAsync(deployment);
        await context.SaveChangesAsync();

        var accountID = new List<int> { deployment.AccountId };
        var modifiedDeploymentEntity = deployment;
        modifiedDeploymentEntity.Status = 2;

        // Act
        await repository.UpdateAccountStatusByContactAsync(accountID, 2);

        // Assert
        var updatedDeployment = await context.DeploymentEntity.FirstOrDefaultAsync();

        Assert.NotNull(updatedDeployment);
        Assert.Equal(modifiedDeploymentEntity.AccountId, updatedDeployment.AccountId);
        Assert.Equal(modifiedDeploymentEntity.DeploymentId, updatedDeployment.DeploymentId);
        Assert.Equal(modifiedDeploymentEntity.Status, updatedDeployment.Status);
    }

    [Fact]
    public async Task GetAccountByNumberAsync_ShouldReturnAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);

        var accountNumber = "number";
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, accountNumber)
            .Create();
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new AccountEventRepository(context);

        var result = await repository.GetAccountByNumberAsync(accountNumber);

        Assert.NotNull(result);
        Assert.Equivalent(account, result);
    }

    [Fact]
    public async Task GetAccountByNumberAsync_WithNoExistingAccount_ShouldThrowInvalidOperationException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);

        var repository = new AccountEventRepository(context);

        var result = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.GetAccountByNumberAsync(string.Empty));

        Assert.NotNull(result);
        Assert.Equal("Sequence contains no elements", result.Message);
    }

    [Fact]
    public async Task DoesAccountExist_ShoudReturnTrue_WhenAccountExists()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var account = _fixture.Create<AccountEntity>();
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountEventRepository(context);

        var result = await repository.DoesAccountExistAsync(account.AccountId);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesAccountExist_ShoudReturnFalse_WhenAccountNotFound()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new AccountEventRepository(context);

        var result = await repository.DoesAccountExistAsync(1);

        Assert.False(result);
    }
}
