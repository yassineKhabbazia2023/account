// <copyright file="DematRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DematRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public DematRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetDematModalClosedDateAsync_WhenNotClosed_ReturnsNull()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DematRepository(context);

        var result = await repository.GetDematModalClosedDateAsync(123, 42);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDematModalClosedDateAsync_WhenClosedByThisContact_ReturnsClosedDate()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC131")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Without(a => a.DematModalClosureEntity)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var closedDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        context.DematModalClosureEntity.Add(new DematModalClosureEntity
        {
            AccountId = account.AccountId,
            ContactId = 42,
            ClosedDate = closedDate,
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DematRepository(context);

        var result = await repository.GetDematModalClosedDateAsync(account.AccountId, 42);

        Assert.Equal(closedDate, result);
    }

    [Fact]
    public async Task GetDematModalClosedDateAsync_WhenClosedByAnotherContact_ReturnsNull()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC134")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Without(a => a.DematModalClosureEntity)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        context.DematModalClosureEntity.Add(new DematModalClosureEntity
        {
            AccountId = account.AccountId,
            ContactId = 42,
            ClosedDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DematRepository(context);

        // Contact 99 n'a jamais fermé son propre modal sur ce compte, même si le contact 42 l'a fait.
        var result = await repository.GetDematModalClosedDateAsync(account.AccountId, 99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CloseDematModalAsync_WhenNotAlreadyClosed_InsertsClosureRow()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC132")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Without(a => a.DematModalClosureEntity)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DematRepository(context);

        await repository.CloseDematModalAsync(account.AccountId, 99);

        var closure = await context.DematModalClosureEntity
            .AsNoTracking()
            .SingleAsync(c => c.AccountId == account.AccountId);
        closure.ContactId.Should().Be(99);
        closure.ClosedDate.Should().NotBe(default);
    }

    [Fact]
    public async Task CloseDematModalAsync_WhenAlreadyClosedBySameContact_DoesNotInsertDuplicateRow()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC133")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Without(a => a.DematModalClosureEntity)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var firstClosedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        context.DematModalClosureEntity.Add(new DematModalClosureEntity
        {
            AccountId = account.AccountId,
            ContactId = 1,
            ClosedDate = firstClosedDate,
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DematRepository(context);

        // Le même contact (1) ferme une seconde fois : idempotent, on garde la date d'origine.
        await repository.CloseDematModalAsync(account.AccountId, 1);

        var closures = await context.DematModalClosureEntity
            .AsNoTracking()
            .Where(c => c.AccountId == account.AccountId)
            .ToListAsync();
        closures.Should().ContainSingle();
        closures[0].ContactId.Should().Be(1);
        closures[0].ClosedDate.Should().Be(firstClosedDate);
    }

    [Fact]
    public async Task CloseDematModalAsync_WhenAnotherContactCloses_InsertsSeparateRow()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountNumber, "ACC135")
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Without(a => a.DematModalClosureEntity)
            .With(a => a.RoleEntity, new List<RoleEntity>())
            .Create();

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var firstClosedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        context.DematModalClosureEntity.Add(new DematModalClosureEntity
        {
            AccountId = account.AccountId,
            ContactId = 1,
            ClosedDate = firstClosedDate,
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DematRepository(context);

        // Le contact 1 a déjà fermé son modal ; le contact 2 ferme le sien, indépendamment.
        await repository.CloseDematModalAsync(account.AccountId, 2);

        var closures = await context.DematModalClosureEntity
            .AsNoTracking()
            .Where(c => c.AccountId == account.AccountId)
            .ToListAsync();
        closures.Should().HaveCount(2);
        closures.Should().Contain(c => c.ContactId == 1 && c.ClosedDate == firstClosedDate);
        closures.Should().Contain(c => c.ContactId == 2);
    }
}
