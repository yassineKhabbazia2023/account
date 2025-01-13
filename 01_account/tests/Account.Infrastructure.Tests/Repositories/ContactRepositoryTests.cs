// <copyright file="ContactRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class ContactRepositoryTests
{
    private readonly DbContextOptions<AccountContext> _contextOptions;
    private readonly IContactRepository _contactRepository;
    private readonly Fixture _fixture;

    public ContactRepositoryTests()
    {
        _contextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _contactRepository = new ContactRepository(new AccountContext(_contextOptions));
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContacts_Will_Return_Only_DifferentThan_Removed()
    {
        using (var context = new AccountContext(_contextOptions))
        {

            var ContactEntitiesWithStatusRemoved = _fixture.CreateMany<ContactEntity>(5).ToList();
            var ContactEntitiesWithStatusInvited = _fixture.CreateMany<ContactEntity>(10).ToList();
            ContactEntitiesWithStatusInvited.ForEach((e) => e.Status = "Invited");
            ContactEntitiesWithStatusRemoved.ForEach((e) => e.Status = "Removed");
            context.ContactEntity.AddRange(ContactEntitiesWithStatusInvited);
            context.ContactEntity.AddRange(ContactEntitiesWithStatusInvited);
            await context.SaveChangesAsync();
            var repository = new ContactRepository(context);
            var contactsViewed = context.ContactEntity.ToList();
            Assert.Equivalent(true, contactsViewed.All(x => x.Status == "Invited"));
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

            // Act
            var action = async () => await _contactRepository.GetContactAsync(2);

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
            PersonaName = "HakounaMatata"
        };

        using (var context = new AccountContext(_contextOptions))
        {
            // arrange
            context.Add(contact);
            context.SaveChanges();

            // Act
            var dbContact = await _contactRepository.GetContactAsync(1);

            // assert
            dbContact.Should().NotBeNull();
            dbContact.Should().BeEquivalentTo(contact);
        }
    }
}
