// <copyright file="RegistryRoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;
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
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
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
            .With(r => r.ContactFlagPortailFactures, true)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.CreateRoleAsync(data, account.AccountId, contact.ContactId, true);

        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result.AccountId);
        Assert.Equal(contact.ContactId, result.ContactId);
        Assert.Equal(data.IsFavorite, result.IsFavorite);
        Assert.Equal(data.RoleSignatory, result.IsSignatory);
        Assert.Equal(data.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
        Assert.Null(result.IsDelegation);

        var insertedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
        Assert.NotNull(insertedRole);
        Assert.Equal(data.ContactFlagPortailFactures, insertedRole.ContactFlagPortailFactures);
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
            .With(r => r.ContactFlagPortailFactures, true)
            .Create();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.CreateRoleAsync(data, accountId, contactId, true);

        Assert.NotNull(result);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(contactId, result.ContactId);
        Assert.Equal(data.IsFavorite, result.IsFavorite);
        Assert.Equal(data.RoleSignatory, result.IsSignatory);
        Assert.Equal(data.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
        Assert.Null(result.IsDelegation);

        var insertedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == accountId && r.ContactId == contactId);
        Assert.NotNull(insertedRole);
        Assert.Equal(data.ContactFlagPortailFactures, insertedRole.ContactFlagPortailFactures);
    }

    [Fact]
    public async Task UpdateRoleContactFlagPortailFacturesAsync_ShouldUpdateContactFlagPortailFactures()
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

        var existingRole = _fixture.Build<RoleEntity>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .With(r => r.ContactFlagPortailFactures, false)
            .Without(r => r.Account)
            .Without(r => r.Contact)
            .Create();
        context.RoleEntity.Add(existingRole);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.UpdateRoleContactFlagPortailFacturesAsync(account.AccountId, contact.ContactId, true);

        Assert.NotNull(result);
        Assert.True(result.ContactFlagPortailFactures);

        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
        Assert.NotNull(updatedRole);
        Assert.True(updatedRole!.ContactFlagPortailFactures);
    }

    [Fact]
    public async Task UpdateRoleContactFlagPortailFacturesAsync_WithMissingRole_ShouldReturnNull()
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

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.UpdateRoleContactFlagPortailFacturesAsync(account.AccountId, contact.ContactId, true);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateRoleIsCustomerRelationAsync_ShouldUpdateIsCustomerRelationAndActionLevel()
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

        var existingRole = _fixture.Build<RoleEntity>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .With(r => r.IsCustomerRelation, false)
            .With(r => r.ActionLevel, 0)
            .Without(r => r.Account)
            .Without(r => r.Contact)
            .Create();
        context.RoleEntity.Add(existingRole);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.UpdateRoleIsCustomerRelationAsync(account.AccountId, contact.ContactId, true, 4);

        Assert.NotNull(result);
        Assert.True(result.IsCustomerRelation);

        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
        Assert.NotNull(updatedRole);
        Assert.True(updatedRole!.IsCustomerRelation);
        Assert.Equal(4, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleIsCustomerRelationAsync_WithMissingRole_ShouldReturnNull()
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

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.UpdateRoleIsCustomerRelationAsync(account.AccountId, contact.ContactId, true, 4);

        Assert.Null(result);
    }

    private static AccountContext CreateSqliteContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        connection.CreateFunction("newid", () => Guid.NewGuid().ToString());
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseSqlite(connection)
                .Options;
        var context = new AccountContext(options);
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
        return context;
    }

    [Fact]
    public async Task UpdateRoleIsSignatoryAsync_RoleExists_Should_UpdateAndReturnTrue()
    {
        using var context = CreateSqliteContext();

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

        var existingRole = _fixture.Build<RoleEntity>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .With(r => r.IsSignatory, false)
            .Without(r => r.Account)
            .Without(r => r.Contact)
            .Create();
        context.RoleEntity.Add(existingRole);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.UpdateRoleIsSignatoryAsync(account.AccountId, contact.ContactId, true);

        Assert.True(result);

        var updatedRole = await context.RoleEntity.AsNoTracking()
            .FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
        Assert.NotNull(updatedRole);
        Assert.True(updatedRole!.IsSignatory);
    }

    [Fact]
    public async Task UpdateRoleIsSignatoryAsync_RoleNotFound_Should_ReturnFalse()
    {
        using var context = CreateSqliteContext();

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

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.UpdateRoleIsSignatoryAsync(account.AccountId, contact.ContactId, true);

        Assert.False(result);
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
            .Without(x => x.Delegation)
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

        context.ChangeTracker.Clear();

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
            .With(a => a.IsActive, true)
            .Without(a => a.RoleEntity)
            .Without(a => a.AddressEntity)
            .Without(a => a.PhoneEntity)
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Without(a => a.DeploymentEntity)
            .Without(a => a.Hub)
            .Without(a => a.Naf)
            .Without(a => a.OfferEligibilityEntity)
            .Without(a => a.Office)
            .Create();
        context.AccountEntity.Add(account);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var data = _fixture.Build<RegistryRoleRemovedEventData>()
            .With(r => r.AccountId, account.AccountId)
            .With(r => r.ContactId, contact.ContactId)
            .Create();

        using (var dbContext = new AccountContext(options))
        {
            var repository = new RegistryRoleEventRepository(dbContext);

            var result = await repository.RemoveRoleAsync(1, 1);

            Assert.False(result);
        }
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(1, 1)]
    public async Task CheckExistingAccountAndContactAsync_WithNonExistingAccountOrContact_ShouldThrowNotFoundException(int accountId, int contactId)
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(options);

        var deployment = _fixture.Build<DeploymentEntity>()
            .With(d => d.DeploymentId, 100)
            .With(d => d.AccountId, 2)
            .Without(d => d.Account)
            .Create();

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 2)
            .With(a => a.DeploymentEntity, deployment)
            .Without(a => a.AddressEntity)
            .Without(a => a.PhoneEntity)
            .Without(a => a.Hub)
            .Without(a => a.Naf)
            .Without(a => a.OfferEligibilityEntity)
            .Without(a => a.Office)
            .Without(a => a.RoleEntity)
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Create();
        context.DeploymentEntity.Add(deployment);
        context.AccountEntity.Add(account);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 2)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await repository.CheckExistingAccountAndContactAsync(accountId, contactId));
    }

    [Fact]
    public async Task GetAccountIdByGuidAsync_ShouldReturnAccountId()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var accountGlobalUniqueId = Guid.NewGuid();
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountGlobalUniqueId, accountGlobalUniqueId)
            .Create();
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.GetAccountIdByGuidAsync(accountGlobalUniqueId);

        Assert.Equal(account.AccountId, result);
    }

    [Fact]
    public async Task GetAccountIdByGuidAsync_WithNoExistingAccount_ShouldThrowNotFoundException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Create<AccountEntity>();
        context.AccountEntity.Add(account);

        var repository = new RegistryRoleEventRepository(context);

        var accountGlobalUniqueId = Guid.NewGuid();
        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAccountIdByGuidAsync(accountGlobalUniqueId));

        Assert.Equal("ACC001", result.Code);
        Assert.Equal($"L'entité avec l'identifiant {accountGlobalUniqueId.ToString()} est introuvable", result.Message);
    }

    [Fact]
    public async Task GetContactByGuidAsync_ShouldReturnContact()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var contactGlobalUniqueId = Guid.NewGuid();
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactGlobalUniqueId, contactGlobalUniqueId)
            .With(c => c.IsActive, true)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.GetContactByGuidAsync(contactGlobalUniqueId);

        Assert.NotNull(result);
        Assert.Equivalent(contact, result);
    }

    [Fact]
    public async Task GetContactByGuidAsync_WithNoExistingAccount_ShouldThrowNotFoundException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var contact = _fixture.Build<ContactEntity>()
          .With(c => c.IsActive, true)
          .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var contactGlobalUniqueId = Guid.NewGuid();
        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetContactByGuidAsync(contactGlobalUniqueId));

        Assert.Equal("ACC002", result.Code);
        Assert.Equal($"Le contact avec l'identifiant {contactGlobalUniqueId.ToString()} est introuvable", result.Message);
    }

    [Fact]
    public async Task GetAccountIdByGuidAsync_WithProspectAccount_ShouldReturnAccountId()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var accountGlobalUniqueId = Guid.NewGuid();
        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountGlobalUniqueId, accountGlobalUniqueId)
            .With(a => a.IsActive, true)
            .With(a => a.AccountType, GlobalConstants.ProspectAccountType)
            .Create();
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.GetAccountIdByGuidAsync(accountGlobalUniqueId);

        Assert.Equal(account.AccountId, result);
    }

    [Fact]
    public async Task CheckExistingAccountAndContactAsync_WithProspectAccount_ShouldNotThrow()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 10)
            .With(a => a.IsActive, true)
            .With(a => a.AccountType, GlobalConstants.ProspectAccountType)
            .Without(a => a.RoleEntity)
            .Without(a => a.PhoneEntity)
            .Without(a => a.AddressEntity)
            .Without(a => a.DeploymentEntity)
            .Without(a => a.RoleLabelEntity)
            .Without(a => a.Delegation)
            .Create();
        context.AccountEntity.Add(account);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 20)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var repository = new RegistryRoleEventRepository(context);

        var exception = await Record.ExceptionAsync(() => repository.CheckExistingAccountAndContactAsync(10, 20));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RemoveRoleAsync_WithProspectAccount_ShouldRemoveRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        using var context = new AccountContext(options);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.AccountId, 1)
            .With(a => a.IsActive, true)
            .With(a => a.AccountType, GlobalConstants.ProspectAccountType)
            .Without(a => a.RoleEntity)
            .Without(a => a.PhoneEntity)
            .Without(a => a.AddressEntity)
            .Without(a => a.DeploymentEntity)
            .Create();

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
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
        context.ChangeTracker.Clear();

        var repository = new RegistryRoleEventRepository(context);

        var result = await repository.RemoveRoleAsync(1, 1);

        Assert.True(result);

        var removedRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == 1 && r.ContactId == 1);
        Assert.Null(removedRole);
    }
}
