// <copyright file="ContactEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactEventRepositoryTests
{
    private readonly DbContextOptions<AccountContext> _contextOptions;
    private readonly Fixture _fixture;

    public ContactEventRepositoryTests()
    {
        _contextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
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
            IsActive = true,
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
            IsActive= true,
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
            IsActive = true
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
            CreationDate = DateTime.UtcNow,
            IsActive = true
        };
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new ContactEventRepository(context);

        var result = await repository.GetContactById(2);

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
            IsActive = true
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

    [Fact]
    public async Task GetContacts_Will_Return_Only_DifferentThan_Removed()
    {
        using (var context = new AccountContext(_contextOptions))
        {
            var contactEntitiesWithStatusRemoved = _fixture.CreateMany<ContactEntity>(5).ToList();
            var contactEntitiesWithStatusInvited = _fixture.CreateMany<ContactEntity>(10).ToList();
            contactEntitiesWithStatusInvited.ForEach((e) => e.Status = "Invited");
            contactEntitiesWithStatusRemoved.ForEach((e) => e.Status = "Removed");
            context.ContactEntity.AddRange(contactEntitiesWithStatusInvited);
            context.ContactEntity.AddRange(contactEntitiesWithStatusInvited);
            await context.SaveChangesAsync();
            var repository = new ContactRepository(context);
            var contactsViewed = context.ContactEntity.ToList();

            Assert.True(contactsViewed.All(x => x.Status == "Invited"));
        }
    }

    [Fact]
    public async Task GetContactAsync_ShouldThrowNotFoundExceptionIfNotExists()
    {
        ContactEntity contact = new ContactEntity()
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 1,
            CreationDate = DateTime.UtcNow,
            Email = "mdibeh@hakouna.com",
            FirstName = "Marc",
            LastName = "Dibeh",
            Type = "Client",
            Status = "Active",
            PersonaName = "HakounaMatata"
        };

        using (var context = new AccountContext(_contextOptions))
        {
            // arrange
            context.Add(contact);
            context.SaveChanges();
            var repository = new ContactEventRepository(context);

            // Act
            var action = async () => await repository.GetContactAsync(2);

            // assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
    }

    [Fact]
    public async Task GetContactAsync_ShouldReturnContactIfExists()
    {
        ContactEntity contact = new ContactEntity()
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 1,
            CreationDate = DateTime.UtcNow,
            Email = "mdibeh@hakouna.com",
            FirstName = "Marc",
            LastName = "Dibeh",
            Type = "Client",
            Status = "Active",
            PersonaName = "HakounaMatata",
            IsActive = true
        };

        using (var context = new AccountContext(_contextOptions))
        {
            // arrange
            context.Add(contact);
            context.SaveChanges();
            var repository = new ContactEventRepository(context);

            // Act
            var dbContact = await repository.GetContactAsync(1);

            // assert
            dbContact.Should().NotBeNull();
            dbContact.Should().BeEquivalentTo(contact);
        }
    }

    [Fact]
    public async Task GetContactAsync_ShouldReturn_InactiveContactsIfRequested()
    {
        var inactiveContacts = _fixture.Build<ContactEntity>().With(x => x.IsActive, false).CreateMany(3);

        var contactId = inactiveContacts.First().ContactId;

        using (var context = new AccountContext(_contextOptions))
        {
            context.AddRange(inactiveContacts);
            context.SaveChanges();

            context.ChangeTracker.Clear();

            var contactRepos = new ContactEventRepository(context);
            var result = await contactRepos.GetContactAsync(contactId, searchDeleted: true);

            result.Should().NotBe(null);
            result.ContactId.Should().Be(contactId);
        }
    }

    [Fact]
    public async Task GetContactByEmailAsync_ShouldThrowNotFoundExceptionIfNotExists()
    {
        ContactEntity contact = new ContactEntity()
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 1,
            CreationDate = DateTime.UtcNow,
            Email = "test@email.com",
            FirstName = "Marc",
            LastName = "Dibeh",
            Type = "Client",
            Status = "Active",
            PersonaName = "HakounaMatata"
        };

        using (var context = new AccountContext(_contextOptions))
        {
            // arrange
            context.Add(contact);
            context.SaveChanges();
            var repository = new ContactEventRepository(context);

            // Act
            var action = async () => await repository.GetContactByEmailAsync(string.Empty);

            // assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
    }

    [Fact]
    public async Task GetContactByEmailAsync_ShouldReturnContactIfExists()
    {
        ContactEntity contact = new ContactEntity()
        {
            ContactGlobalUniqueId = Guid.NewGuid(),
            ContactId = 1,
            CreationDate = DateTime.UtcNow,
            Email = "test@email.com",
            FirstName = "Marc",
            LastName = "Dibeh",
            Type = "Client",
            Status = "Active",
            PersonaName = "HakounaMatata",
            IsActive = true
        };

        using (var context = new AccountContext(_contextOptions))
        {
            // arrange
            context.Add(contact);
            context.SaveChanges();
            var repository = new ContactEventRepository(context);

            // Act
            var dbContact = await repository.GetContactByEmailAsync("test@email.com");

            // assert
            dbContact.Should().NotBeNull();
            dbContact.Should().BeEquivalentTo(contact);
        }
    }
}
