// <copyright file="ContactEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactEventRepositoryTests
{
    private readonly Fixture _fixture;

    public ContactEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    [Fact]
    public async Task CreateContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new ContactEventRepository(context);
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

        // Act
        await repository.CreateContactAsync(contactEntity);

        // Assert
        var addedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(addedContact);
        Assert.Equal(contactEntity.ContactId, addedContact.ContactId);
        Assert.Equal(contactEntity.ContactGlobalUniqueId, addedContact.ContactGlobalUniqueId);
        Assert.Equal(contactEntity.FirstName, addedContact.FirstName);
        Assert.Equal(contactEntity.LastName, addedContact.LastName);
        Assert.Equal(contactEntity.Email, addedContact.Email);
        Assert.Equal(contactEntity.PersonaName, addedContact.PersonaName);
        Assert.Equal(contactEntity.Status, addedContact.Status);
        Assert.Equal(contactEntity.Type, addedContact.Type);
        Assert.Equal(contactEntity.CreationDate, addedContact.CreationDate);
    }

    [Fact]
    public async Task UpdateContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new ContactEventRepository(context);
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

        await context.ContactEntity.AddAsync(contactEntity);
        await context.SaveChangesAsync();

        var modifiedContactEntity = new ContactEntity
        {
            ContactId = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@test.com",
            PersonaName = "Collab RS",
            Status = ContactStatus.Connected.ToString(),
            Type = "collaborator",
            CreationDate = DateTime.Parse("2024-04-16T09:19:16Z"),
            ContactGlobalUniqueId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF"),
        };

        // Act
        await repository.UpdateContactAsync(modifiedContactEntity);

        // Assert
        var updatedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(updatedContact);
        Assert.Equal(modifiedContactEntity.ContactId, updatedContact.ContactId);
        Assert.Equal(modifiedContactEntity.ContactGlobalUniqueId, updatedContact.ContactGlobalUniqueId);
        Assert.Equal(modifiedContactEntity.FirstName, updatedContact.FirstName);
        Assert.Equal(modifiedContactEntity.LastName, updatedContact.LastName);
        Assert.Equal(modifiedContactEntity.Email, updatedContact.Email);
        Assert.Equal(modifiedContactEntity.PersonaName, updatedContact.PersonaName);
        Assert.Equal(modifiedContactEntity.Status, updatedContact.Status);
        Assert.Equal(modifiedContactEntity.Type, updatedContact.Type);
        Assert.Equal(modifiedContactEntity.CreationDate, updatedContact.CreationDate);
    }

    [Fact]
    public async Task RevokContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new ContactEventRepository(context);
        var contactEntity = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PersonaName = "Collab GS",
            Status = ContactStatus.Connected.ToString(),
            Type = "collaborator",
            CreationDate = DateTime.Parse("2024-04-16T09:19:16Z"),
            ContactGlobalUniqueId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF"),
        };

        await context.ContactEntity.AddAsync(contactEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveContactAsync(contactId: 1);

        // Assert
        var updatedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(updatedContact);
        Assert.Equal(ContactStatus.Removed.ToString(), updatedContact.Status);
    }

    [Fact]
    public async Task GetAccountQueryByContactId_WithContactId_ShouldReturnAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new ContactEventRepository(context);
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
            SourceAccountNumber = "29999999"
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
        Assert.Equal(accountEntity.SourceAccountNumber, accountByContact.SourceAccountNumber);
    }

    [Fact]
    public async Task UpdateAccountStatusByContactAsync_WithAccountIds_ShouldReturnUpdatedAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new ContactEventRepository(context);
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
}
