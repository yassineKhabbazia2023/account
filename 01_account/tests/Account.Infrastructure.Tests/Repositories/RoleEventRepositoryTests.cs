// <copyright file="RoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class RoleEventRepositoryTests
{
    private readonly Mock<ILogger<RoleEventRepository>> _loggerMock = new();

    [Fact]
    public async Task DeleteContactRolesAsync_ShouldDeleteAllContactRoles()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context, _loggerMock.Object);
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
    public async Task CreateRoleForAutomaticDelegations_ShouldCreateRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context, _loggerMock.Object);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = default,
            AccountNumber = "1234",
            LegalName = "legal",
            CreatedBy = "pas oim"
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

        var result = await repository.CreateRoleForAutomaticDelegations(delegator.ContactId, account.AccountId);

        Assert.NotEmpty(result);
        Assert.Single(result);

        var data = result.FirstOrDefault() !;
        Assert.Equal(delegatee.ContactId, data.ContactId);
        Assert.Equal(delegatee.ContactGlobalUniqueId, data.ContactGlobalUniqueId);
        Assert.Equal(account.AccountId, data.AccountId);
        Assert.Equal(account.AccountGlobalUniqueId, data.AccountGlobalUniqueId);
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
        var repository = new RoleEventRepository(context, _loggerMock.Object);

        var account = new AccountEntity
        {
            AccountId = 1,
            AccountGlobalUniqueId = default,
            AccountNumber = "1234",
            LegalName = "legal",
            CreatedBy = "oim"
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

        var result = await repository.CreateRoleForAutomaticDelegations(delegator.ContactId, account.AccountId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateRoleForNewContact_ShouldCreateRole()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context, _loggerMock.Object);

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

        var result = await repository.CreateRoleForNewContact(contact.ContactId, account.AccountNumber);

        Assert.NotNull(result);

        Assert.Equal(contact.ContactId, result.ContactId);
        Assert.Equal(account.AccountId, result.AccountId);
    }
}
