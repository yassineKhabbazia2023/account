// <copyright file="ContactRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class ContactRepositoryTests
{
    private readonly DbContextOptions<AccountContext> _contextOptions;
    private readonly Fixture _fixture;

    public ContactRepositoryTests()
    {
        _contextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
    }

    [Fact]
    public async Task GetContactByIdAsync_ShouldReturnContact()
    {
        var contact = _fixture.Create<ContactEntity>();

        using var context = new AccountContext(_contextOptions);

        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new ContactRepository(context);

        var result = await repository.GetContactByIdAsync(contact.ContactId);

        Assert.NotNull(result);
        Assert.Equal(contact.ContactId, result.ContactId);
    }

    [Fact]
    public async Task GetContactByIdAsync_ShouldThrowNotFoundException_WhenContactNotFound()
    {
        using var context = new AccountContext(_contextOptions);
        context.ChangeTracker.Clear();

        var repository = new ContactRepository(context);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetContactByIdAsync(1));

        Assert.Equal("ACC002", result.Code);
        Assert.Equal("Le contact avec l'identifiant 1 est introuvable", result.Message);
    }
}
