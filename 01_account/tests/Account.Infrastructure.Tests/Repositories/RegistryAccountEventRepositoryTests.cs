// <copyright file="RegistryAccountEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class RegistryAccountEventRepositoryTests
{
    private readonly RegistryAccountCreatedEventData _accountCreatedData = new RegistryAccountCreatedEventData
    {
        Id = Guid.NewGuid(),
        AccountNumber = "69696969",
        DeploymentStatus = DeploymentStatus.ToDeploy.ToString(),
        LegalName = "Johny Pizza ?"
    };

    private readonly RegistryAccountUpdatedEventData _accountUpdatedEventData = new RegistryAccountUpdatedEventData
    {
        Id = Guid.NewGuid(),
        AccountNumber = "69696969",
        DeploymentStatus = DeploymentStatus.ToDeploy.ToString(),
    };

    private readonly AccountEntity _accountEntity = new AccountEntity
    {
        AccountId = 1,
        AccountGlobalUniqueId = Guid.NewGuid(),
        AccountNumber = "4242424242",
        LegalName = "Jooooohnnnnyyyy Piza",
        CreatedBy = "Me",
        Email = "me@me.fr",
    };

    [Fact]
    public async Task CreateAccountAsync_WithAccountData_ShouldCreateAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new RegistryAccountEventRepository(context);

        // Act
        var detail = await repository.CreateAccountAsync(_accountCreatedData);
        var addedAccount = await context.AccountEntity.FirstOrDefaultAsync();
        var addedDetail = addedAccount!.MapToAccountDetail();

        // Assert
        Assert.NotNull(addedAccount);
        Assert.Equivalent(detail, addedDetail);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithAccountData_ShouldCreateAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new RegistryAccountEventRepository(context);
        _accountEntity.AccountGlobalUniqueId = _accountUpdatedEventData.Id;
        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        // Act
        var detail = await repository.UpdateAccountAsync(_accountUpdatedEventData);
        var updatedAccount = await context.AccountEntity.FirstOrDefaultAsync();
        var updatedDetail = updatedAccount!.MapToAccountDetail();

        // Assert
        Assert.NotNull(updatedAccount);
        Assert.Equivalent(detail, updatedDetail);
        Assert.NotNull(updatedAccount.UpdatedDate);
    }

    [Fact]
    public async Task RemoveAccountAsync_WithAccountData_ShouldRemoveAccount()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new RegistryAccountEventRepository(context);
        _accountEntity.AccountGlobalUniqueId = _accountUpdatedEventData.Id;
        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveAccountAsync(_accountUpdatedEventData.Id);
        var removedAccount = await context.AccountEntity.FirstOrDefaultAsync();
        var removedAccountDetail = removedAccount!.MapToAccountDetail();

        // Assert
        Assert.NotNull(removedAccount);
        Assert.Equal((int)DeploymentStatus.Revoked, removedAccountDetail!.Deployment!.Status);
    }
}
