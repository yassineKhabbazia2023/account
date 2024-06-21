// <copyright file="MapToRoleEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Mappers.Events;

public class MapToRoleEntityTests
{
    [Fact]
    public async Task ToRoleEntity_MapsCorrectly()
    {
        // Arrange
        var source = new RegistryRoleCreatedEventData
        {
            AccountId = Guid.NewGuid(),
            AccountNumber = "1289090",
            ContactId = Guid.NewGuid(),
            Email = "email@test.fr",
            IsFavorite = true,
            RoleDelegataireEmail = "delegataire@email.fr",
            RoleSignatory = false
        };

        DbContextOptions<AccountContext> dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(dbContextOptions);

        context.AccountEntity.Add(new AccountEntity()
        {
            AccountGlobalUniqueId = source.AccountId,
            AccountNumber = "19890827",
            CreatedBy = "created By Me",
            LegalName = "Test legalname",
            AccountId = 123
        });

        context.ContactEntity.Add(new ContactEntity()
        {
            ContactGlobalUniqueId = source.ContactId,
            Email = source.Email,
            FirstName = "Test firstname",
            LastName = "Test lastname",
            PersonaName = "Test persona",
            Type = "Test type",
            ContactId = 123
        });

        await context.SaveChangesAsync();

        var contactSource = context.ContactEntity.First(c => c.ContactGlobalUniqueId == source.ContactId);
        var accountSource = context.AccountEntity.First(c => c.AccountGlobalUniqueId == source.AccountId);

        // Act
        var result = source.ToRoleEntity(context);

        // Assert
        Assert.Equal(contactSource.ContactId, result.ContactId);
        Assert.Equal(accountSource.AccountId, result.AccountId);
        Assert.Equal(source.IsFavorite, result.IsFavorite);
        Assert.Equal(source.RoleSignatory, result.IsSignatory);
    }

    [Fact]
    public void ToRoleEntity_Throw_Exception()
    {
        // Arrange
        var source = new RegistryRoleCreatedEventData
        {
            AccountId = Guid.NewGuid(),
            AccountNumber = "1289090",
            ContactId = Guid.NewGuid(),
            Email = "email@test.fr",
            IsFavorite = true,
            RoleDelegataireEmail = "delegataire@email.fr",
            RoleSignatory = false
        };

        DbContextOptions<AccountContext> dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(dbContextOptions);

        // Act
        Func<RoleEntity> action = () => source.ToRoleEntity(context);

        // Assert
        var exception = Assert.Throws<NotFoundException>(() => action());
        Assert.Equal(string.Format(Errors.NotFoundRoleMessage, source.ContactId, source.AccountId), exception.Message);
    }

    [Fact]
    public void ToCreateRoleRequest_MapsCorrectly()
    {
        // Arrange
        var roleEntity = new RoleEntity
        {
            ContactId = 100,
            AccountId = 122,
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true
        };

        // Act
        var createdRole = roleEntity.ToCreateRoleRequest();

        // Assert
        Assert.Equal(createdRole.ContactId, roleEntity.ContactId);
        Assert.Equal(createdRole.AccountId, roleEntity.AccountId);
        Assert.Equal(createdRole.IsFavorite, roleEntity.IsFavorite);
        Assert.Equal(createdRole.IsSignatory, roleEntity.IsSignatory);
    }
}
