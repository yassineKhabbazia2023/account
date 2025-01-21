// <copyright file="ContactEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactEventRepositoryTests
{
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
            IsActive = true
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
        Assert.NotNull(updatedContact.LastUpdateDate);
    }

    [Fact]
    public async Task RemoveContactAsync_WithContactData_ShouldCreateContact()
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
        // I have ignored query filters because by default removed entities will not be returned
        var updatedContact = await context.ContactEntity.IgnoreQueryFilters().FirstOrDefaultAsync();

        Assert.NotNull(updatedContact);
        Assert.Equal(ContactStatus.Removed.ToString(), updatedContact.Status);
        Assert.False(updatedContact.IsActive);
        Assert.NotNull(updatedContact.LastUpdateDate);
    }

    [Fact]
    public async Task GetContactById_ShouldReturnContact()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);

        var contact = new ContactEntity
        {
            ContactId = 2,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Pierre",
            Email = "jeanpierre@kpmg.fr",
            PersonaName = "collaborator",
            Status = ContactStatus.Connected.ToString(),
            Type = "collaborator",
            CreationDate = DateTime.UtcNow
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new ContactEventRepository(context);

        var result = repository.GetContactById(2);

        Assert.NotNull(result);
        Assert.Equivalent(contact, result);
    }

    [Fact]
    public async Task DoesContactExistAsync_WithExistingContact_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);

        var contact = new ContactEntity
        {
            ContactId = 3,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Jean",
            LastName = "Pierre",
            Email = "jeanpierre@kpmg.fr",
            PersonaName = "collaborator",
            Type = "collaborator",
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new ContactEventRepository(context);

        var result = await repository.DoesContactExistAsync(3);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesContactExistAsync_WithNoExistingContact_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);

        var repository = new ContactEventRepository(context);

        var result = await repository.DoesContactExistAsync(3);

        Assert.False(result);
    }
}
