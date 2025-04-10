// <copyright file="RoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class RoleEventRepositoryTests
{
    [Fact]
    public async Task DeleteContactRolesAsync_ShouldDeleteAllContactRoles()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context);
        var role1 = new RoleEntity
        {
            ContactId = 1,
            AccountId = 2,
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true
        };
        var role2 = new RoleEntity
        {
            ContactId = 1,
            AccountId = 4,
            IsDelegation = false,
            IsFavorite = false,
            IsSignatory = true
        };

        await context.RoleEntity.AddRangeAsync(new List<RoleEntity> { role1, role2 });
        await context.SaveChangesAsync();

        var result = await repository.DeleteContactRolesAsync(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldThrowBadRequest_IfRoleAlreadyExists()
    {
        var fixture = new Fixture();
        fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var role = new RoleEntity { AccountId = 1, ContactId = 2 };

        var account = fixture.Build<AccountEntity>()
            .With(x => x.AccountId, 1)
            .Without(x => x.RoleEntity)
            .Without(x => x.Office)
            .Without(x => x.OfficeId)
            .Create();

        var contact = fixture.Build<ContactEntity>()
            .With(x => x.ContactId, 2)
            .Without(x => x.RoleEntity)
            .With(c => c.IsActive, true)
            .Create();

        var dbOptions = new DbContextOptionsBuilder<AccountContext>().
            UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        CreateRoleRequest createRoleRequest = new CreateRoleRequest { AccountId = account.AccountId, ContactId = contact.ContactId };

        using (var context = new AccountContext(dbOptions))
        {
            context.ContactEntity.Add(contact);
            context.AccountEntity.Add(account);
            context.SaveChanges();
            context.RoleEntity.Add(role);
            context.SaveChanges();
            using (var newContext = new AccountContext(dbOptions))
            {
                var repos = new RoleRepository(newContext);
                var action = async () => await repos.CreateRoleAsync(createRoleRequest);

                await action.Should().ThrowAsync<ConflictException>();
            }
        }
    }

    [Fact]
    public async Task CreateRoleForAutomaticDelegations_ShouldCreateRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = default,
            AccountNumber = "1234",
            LegalName = "legal",
            CreatedBy = "pas oim",
            IsActive = true
        };
        context.AccountEntity.Add(account);

        var delegator = new ContactEntity
        {
            ContactId = 1,
            ContactGlobalUniqueId = default,
            FirstName = "jean",
            LastName = "pierre",
            Email = "jp@kpmg.fr",
            PersonaName = "collaborator",
            Type = "collaborator",
            IsActive = true
        };
        var delegatee = new ContactEntity
        {
            ContactId = 2,
            ContactGlobalUniqueId = default,
            FirstName = "pierre",
            LastName = "jean",
            Email = "pj@kpmg.fr",
            PersonaName = "client",
            Type = "customer",
            IsActive = true
        };
        context.ContactEntity.AddRange(new List<ContactEntity> { delegator, delegatee });

        var role = new RoleEntity
        {
            Contact = delegator,
            Account = account,
            IsSignatory = true,
            IsFavorite = true,
            IsDelegation = false
        };
        context.RoleEntity.Add(role);

        var delegation = new DelegationEntity
        {
            Delegator = delegator,
            Delegatee = delegatee,
            StartDate = DateTime.UtcNow,
            IsFullDelegation = true,
            IsAutomaticDelegation = true,
            Status = DelegationStatus.Enabled.ToString()
        };
        context.DelegationEntity.Add(delegation);
        await context.SaveChangesAsync();

        var result = await repository.CreateRoleForAutomaticDelegationsAsync(delegator.ContactId, account.AccountId);

        Assert.NotEmpty(result);
        Assert.Single(result);

        var data = result.FirstOrDefault()!;
        Assert.Equal(delegatee.ContactId, data.ContactId);
        Assert.Equal(delegatee.ContactGlobalUniqueId, data.ContactGlobalUniqueId);
        Assert.Equal(account.AccountId, data.AccountId);
        Assert.Equal(account.AccountGlobalUniqueId, data.AccountGlobalUniqueId);
        Assert.Equal(delegator.ContactId, data.DelegatorId);
        Assert.False(data.IsSignatory);
        Assert.False(data.IsFavorite);
        Assert.True(data.IsDelegation);
    }

    [Fact]
    public async Task CreateRoleForAutomaticDelegations_ShouldNotCreateRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = default,
            AccountNumber = "1234",
            LegalName = "legal",
            CreatedBy = "oim",
            IsActive = true
        };
        context.AccountEntity.Add(account);

        var delegator = new ContactEntity
        {
            ContactId = 1,
            ContactGlobalUniqueId = default,
            FirstName = "jean",
            LastName = "pierre",
            Email = "jp@kpmg.fr",
            PersonaName = "collaborator",
            Type = "collaborator"
        };
        var delegatee = new ContactEntity
        {
            ContactId = 2,
            ContactGlobalUniqueId = default,
            FirstName = "pierre",
            LastName = "jean",
            Email = "pj@kpmg.fr",
            PersonaName = "client",
            Type = "customer"
        };
        context.ContactEntity.AddRange(new List<ContactEntity> { delegator, delegatee });

        var roleDelegator = new RoleEntity
        {
            Contact = delegator,
            Account = account,
            IsSignatory = true,
            IsFavorite = true,
            IsDelegation = false
        };
        var roleDelegatee = new RoleEntity
        {
            Contact = delegatee,
            Account = account,
            IsSignatory = false,
            IsFavorite = false,
            IsDelegation = false
        };
        context.RoleEntity.AddRange(new List<RoleEntity> { roleDelegator, roleDelegatee });

        var delegation = new DelegationEntity
        {
            Delegator = delegator,
            Delegatee = delegatee,
            StartDate = DateTime.UtcNow,
            IsFullDelegation = true,
            IsAutomaticDelegation = true,
            Status = DelegationStatus.Enabled.ToString()
        };
        context.DelegationEntity.Add(delegation);
        await context.SaveChangesAsync();

        var result = await repository.CreateRoleForAutomaticDelegationsAsync(delegator.ContactId, account.AccountId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateRoleForNewContact_ShouldCreateRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = default,
            AccountNumber = "1234",
            LegalName = "legal",
            CreatedBy = "pas oim"
        };
        context.AccountEntity.Add(account);

        var contact = new ContactEntity
        {
            ContactId = 1,
            ContactGlobalUniqueId = default,
            FirstName = "jean",
            LastName = "pierre",
            Email = "jp@kpmg.fr",
            PersonaName = "collaborator",
            Type = "collaborator"
        };
        context.ContactEntity.Add(contact);

        await context.SaveChangesAsync();

        var result = await repository.CreateRoleForNewContactAsync(contact.ContactId, account.AccountId);

        Assert.NotNull(result);

        Assert.Equal(contact.ContactId, result.ContactId);
        Assert.Equal(account.AccountId, result.AccountId);
    }

    [Fact]
    public async Task DoesRoleExist_WithExistingRole_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var role = new RoleEntity
        {
            AccountId = 1,
            ContactId = 1,
            IsSignatory = true,
            IsFavorite = true,
            IsDelegation = false
        };
        context.RoleEntity.Add(role);
        await context.SaveChangesAsync();

        var repository = new RoleEventRepository(context);

        var result = await repository.DoesRoleExistAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesRoleExist_WithNoExistingRole_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);

        var repository = new RoleEventRepository(context);

        var result = await repository.DoesRoleExistAsync(1, 1);

        Assert.False(result);
    }
}
