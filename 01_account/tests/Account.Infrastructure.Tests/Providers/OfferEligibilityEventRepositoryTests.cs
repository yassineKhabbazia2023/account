// <copyright file="OfferEligibilityEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class OfferEligibilityEventRepositoryTests
{
    private readonly Fixture _fixture;

    public OfferEligibilityEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateOfferEligibilityAsync_Nominal()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var offerEligibility = _fixture.Create<OfferEligibilityEntity>();

        var repository = new OfferEligibilityEventRepository(context);

        await repository.CreateOfferEligibilityAsync(offerEligibility);

        var result = await context.OfferEligibilityEntity.FirstAsync(x => x.AccountId == offerEligibility.AccountId);

        Assert.NotNull(result);
        Assert.Equivalent(offerEligibility, result);
    }

    [Fact]
    public async Task DoesOfferEligibilityExistsAsync_ShouldReturnTrue_WhenOfferEligibilityExists()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var offerEligibility = _fixture.Create<OfferEligibilityEntity>();
        context.OfferEligibilityEntity.Add(offerEligibility);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new OfferEligibilityEventRepository(context);

        var result = await repository.DoesOfferEligibilityExistsAsync(offerEligibility.AccountId);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesOfferEligibilityExistsAsync_ShouldReturnFalse_WhenOfferEligibilityNotFound()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);

        var repository = new OfferEligibilityEventRepository(context);

        var result = await repository.DoesOfferEligibilityExistsAsync(1);

        Assert.False(result);
    }
}
