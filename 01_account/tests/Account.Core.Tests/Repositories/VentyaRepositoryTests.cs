// <copyright file="VentyaRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Core.Tests.Repositories;

public class VentyaRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public VentyaRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetAccountEmailAsync_WhenAccountNotFound_ReturnsNotFound()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new VentyaRepository(context);

        var result = await repository.GetAccountEmailAsync("ACC404");

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

        var result = await repository.GetAccountEmailAsync("ACC123");

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

        var result = await repository.GetAccountEmailAsync("ACC124");

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

        var result = await repository.GetSsoContactIdAsync("ACC125");

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

        var result = await repository.GetSsoContactIdAsync("ACC126");

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

        var result = await repository.GetSsoContactIdAsync("ACC127");

        Assert.Null(result);
    }
}
