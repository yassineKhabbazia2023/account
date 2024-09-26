// <copyright file="ContactRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
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

    public ContactRepositoryTests()
    {
        _contextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _contactRepository = new ContactRepository(new AccountContext(_contextOptions));
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
