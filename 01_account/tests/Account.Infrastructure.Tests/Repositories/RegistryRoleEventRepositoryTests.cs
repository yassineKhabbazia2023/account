// <copyright file="RegistryRoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

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
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var data = _fixture.Build<RegistryRoleCreatedEventData>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.CreateRoleAsync(data, account.AccountId, contact.ContactId);

        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(contact.ContactId, result.ContactId);
        Assert.Equal(data.IsFavorite, result.IsFavorite);
        Assert.Equal(data.RoleSignatory, result.IsSignatory);
        Assert.Null(result.IsDelegation);

        var insertedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
        Assert.NotNull(insertedRole);
    }

    [Fact]
    public async Task CreateRoleAsync_WithOveriddenValues_ShouldCreateRole()
    {
        var contactId = 602;
        var accountId = 25;

        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .Without(a => a.RoleEntity)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var data = _fixture.Build<RegistryRoleCreatedEventData>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.CreateRoleAsync(data, accountId, contactId);

        Assert.NotNull(result);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(contactId, result.ContactId);
        Assert.Equal(data.IsFavorite, result.IsFavorite);
        Assert.Equal(data.RoleSignatory, result.IsSignatory);
        Assert.Null(result.IsDelegation);

        var insertedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == accountId && r.ContactId == contactId);
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
            .Without(x => x.PhoneEntity)
            .Without(x => x.AddressEntity)
            .Without(x => x.DeploymentEntity)
            .Create();

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .Without(x => x.RoleEntity)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .With(c => c.IsActive, true)
            .Create();


        var role = _fixture.Build<RoleEntity>()
            .With(r => r.AccountId, 1)
            .With(r => r.ContactId, 1)
            .Without(r => r.Account)
            .Without(r => r.Contact)
            .Create();

        context.AccountEntity.Add(account);
        context.ContactEntity.Add(contact);
        context.SaveChanges();
        context.RoleEntity.Add(role);
        context.SaveChanges();

        var data = _fixture.Build<RegistryRoleRemovedEventData>()
            .With(r => r.AccountId, 1)
            .With(r => r.ContactId, 1)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.RemoveRoleAsync(1, 1);

        Assert.True(result);

        var removedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);

        Assert.Null(removedRole);
    }

    [Fact]
    public async Task RemoveRoleAsync_WithNonExistingRole_ShouldReturnFalse()
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
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var data = _fixture.Build<RegistryRoleRemovedEventData>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.RemoveRoleAsync(1, 1);

        Assert.False(result);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(1, 1)]
    public async Task CheckExistingAccountAndContactAsync_WithNonExistingAccountOrContact_ShouldThrowNotFoundException(int accountId, int contactId)
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 2)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 2)
            .With(c => c.IsActive, true)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CheckExistingAccountAndContactAsync(accountId, contactId));

        Assert.Equal(Errors.NotFoundRoleCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundRoleMessage, contactId, accountId), result.Message);
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
            .With(c => c.IsActive, true)
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
        var contact = _fixture.Build<ContactEntity>()
          .With(c => c.IsActive, true)
          .Create();
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
