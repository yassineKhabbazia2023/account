// <copyright file="RegistryRoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class RegistryRoleEventRepositoryTests
{
    private readonly Fixture _fixture;

    public RegistryRoleEventRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldCreateRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .Without(a => a.RoleEntity)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .Without(c => c.RoleEntity)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var data = _fixture.Build<RegistryRoleCreatedEventData>()
            .With(r => r.AccountId, account.AccountGlobalUniqueId)
            .With(r => r.ContactId, contact.ContactGlobalUniqueId)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.CreateRoleAsync(data);

        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(data.AccountId, result.AccountGlobalUniqueId);
        Assert.Equal(contact.ContactId, result.ContactId);
        Assert.Equal(data.ContactId, result.ContactGlobalUniqueId);
        Assert.Equal(data.IsFavorite, result.IsFavorite);
        Assert.Equal(data.RoleSignatory, result.IsSignatory);
        Assert.Null(result.IsDelegation);

        var insertedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
        Assert.NotNull(insertedRole);
    }

    [Fact]
    public async Task RemoveRoleAsync_ShouldRemoveRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 1)
            .Without(a => a.RoleEntity)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .Without(c => c.RoleEntity)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        var role = _fixture.Build<RoleEntity>()
            .With(r => r.AccountId, 1)
            .With(r => r.ContactId, 1)
            .With(r => r.Account, account)
            .With(r => r.Contact, contact)
            .Create();
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();

        var data = _fixture.Build<RegistryRoleRemovedEventData>()
            .With(r => r.AccountId, account.AccountGlobalUniqueId)
            .With(r => r.ContactId, contact.ContactGlobalUniqueId)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        (var accountIdResult, var contactIdResult) = await repository.RemoveRoleAsync(data);

        Assert.Equal(account.AccountId, accountIdResult);
        Assert.Equal(contact.ContactId, contactIdResult);

        var removedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);

        Assert.Null(removedRole);
    }

    [Fact]
    public async Task RemoveRoleAsync_WithNonExistingRole_ShouldReturn0()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .Without(a => a.RoleEntity)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .Without(c => c.RoleEntity)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var data = _fixture.Build<RegistryRoleRemovedEventData>()
            .With(r => r.AccountId, account.AccountGlobalUniqueId)
            .With(r => r.ContactId, contact.ContactGlobalUniqueId)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        (var accountIdResult, var contactIdResult) = await repository.RemoveRoleAsync(data);

        Assert.Equal(0, accountIdResult);
        Assert.Equal(0, contactIdResult);
    }

    [Fact]
    public async Task GetAccountIdContactIdAsync_ShouldReturnAccountIdAndContactId()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var accountGlobalUniqueId = Guid.NewGuid();
        var contactGlobalUniqueId = Guid.NewGuid();
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountGlobalUniqueId, accountGlobalUniqueId)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactGlobalUniqueId, contactGlobalUniqueId)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        (var accountIdResult, var contactIdResult) = await repository.GetAccountIdContactIdAsync(accountGlobalUniqueId, contactGlobalUniqueId);

        Assert.Equal(account.AccountId, accountIdResult);
        Assert.Equal(contact.ContactId, contactIdResult);
    }

    [Fact]
    public async Task GetAccountIdContactIdAsync_WithNonExistingAccountOrContact_ShouldThrowNotFoundException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Create<AccountEntity>();
        context.AccountEntity.Add(account);
        var contact = _fixture.Create<ContactEntity>();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var accountGlobalUniqueId = Guid.NewGuid();
        var contactGlobalUniqueId = Guid.NewGuid();
        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAccountIdContactIdAsync(accountGlobalUniqueId, contactGlobalUniqueId));

        Assert.Equal(Errors.NotFoundRoleCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundRoleMessage, contactGlobalUniqueId, accountGlobalUniqueId), result.Message);
    }
}
