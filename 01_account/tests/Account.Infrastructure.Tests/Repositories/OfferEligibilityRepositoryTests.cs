// <copyright file="OfferEligibilityRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class OfferEligibilityRepositoryTests
{
    private readonly DbContextOptions<AccountContext> _dbOptions;

    public OfferEligibilityRepositoryTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetOfferEligibilityByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        using var context = new AccountContext(_dbOptions);
        var entity = new OfferEligibilityEntity
        {
            AccountId = 123,
            ApprovedBy = "admin@test.fr",
            ApprovedDate = DateTime.UtcNow,
            OfferName = "name",
            IsEligible = true
        };
        context.OfferEligibilityEntity.Add(entity);
        await context.SaveChangesAsync();

        var repo = new OfferEligibilityRepository(context);

        // Act
        var result = await repo.GetOfferEligibilityByIdAsync(123);

        // Assert
        result.Should().NotBeNull();
        result!.AccountId.Should().Be(123);
        result.ApprovedBy.Should().Be("admin@test.fr");
        result.IsEligible.Should().BeTrue();
    }

    [Fact]
    public async Task GetOfferEligibilityByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        using var context = new AccountContext(_dbOptions);
        var repo = new OfferEligibilityRepository(context);

        var result = await repo.GetOfferEligibilityByIdAsync(999);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("2024-01-01", true)]
    [InlineData(null, false)]
    [InlineData("0001-01-01", false)]
    public async Task IsOfferEligibilityActiveAsync_ShouldReturnExpected(string? approvedDateString, bool expected)
    {
        DateTime? approvedDate = approvedDateString is null
            ? null
            : DateTime.Parse(approvedDateString);

        using var context = new AccountContext(_dbOptions);
        var entity = new OfferEligibilityEntity
        {
            AccountId = 555,
            ApprovedDate = approvedDate,
            IsEligible = true,
            OfferName = "name"
        };
        context.OfferEligibilityEntity.Add(entity);
        await context.SaveChangesAsync();

        var repo = new OfferEligibilityRepository(context);

        var result = await repo.IsOfferEligibilityActiveAsync(555);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task UpdateOfferEligibilityAsync_ShouldUpdateAndReturn()
    {
        using var context = new AccountContext(_dbOptions);
        var entity = new OfferEligibilityEntity
        {
            AccountId = 42,
            IsEligible = true,
            OfferName = "name"
        };
        context.OfferEligibilityEntity.Add(entity);
        await context.SaveChangesAsync();

        var repo = new OfferEligibilityRepository(context);

        // Act
        var result = await repo.UpdateOfferEligibilityAsync(42, "admin@test.com");

        // Assert
        result.Should().NotBeNull();
        result.ApprovedBy.Should().Be("admin@test.com");
        result.IsEligible.Should().BeFalse();
        result.ApprovedDate.Should().NotBeNull();

        var dbEntity = await context.OfferEligibilityEntity.FirstAsync(o => o.AccountId == 42);
        dbEntity.IsEligible.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOfferEligibilityAsync_ShouldThrow_WhenNotFound()
    {
        using var context = new AccountContext(_dbOptions);
        var repo = new OfferEligibilityRepository(context);

        Func<Task> act = async () => await repo.UpdateOfferEligibilityAsync(999, "admin@test.com");

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{999}*");
    }
}
