// <copyright file="RegistryAccountEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Azure.Amqp.Framing;
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
        // Arrange
        var fixture = new Fixture();
        var data = fixture.Build<RegistryAccountCreatedEventData>()
            .With(x => x.AccountNafIdentifier, "1")
            .With(x => x.AccountStaffSize, "1")
            .With(x => x.Turnover, "0.1")
            .With(x => x.DeploymentStatus, "1")
            .Create();
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new RegistryAccountEventRepository(context);

        // Act
        var detail = await repository.CreateAccountAsync(data!);
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
        var guid = default(Guid);

        // Arrange
        var fixture = new Fixture();
        var source = fixture.Build<RegistryAccountCreatedEventData>()
            .With(x => x.AccountGlobalUniqueIdentifier, guid)
            .With(x => x.AccountNafIdentifier, "1")
            .With(x => x.AccountStaffSize, "1")
            .With(x => x.Turnover, "0.1")
            .With(x => x.DeploymentStatus, "1")
            .Create();

        var destination = fixture.Build<RegistryAccountUpdatedEventData>()
            .With(x => x.AccountGlobalUniqueIdentifier, guid)
            .With(x => x.AccountNafIdentifier, "1")
            .With(x => x.AccountStaffSize, "1")
            .With(x => x.Turnover, "0.1")
            .With(x => x.DeploymentStatus, "1")
            .Create();

        using var context = new AccountContext(options);
        var repository = new RegistryAccountEventRepository(context);
        await repository.CreateAccountAsync(source);

        // Act
        var detail = await repository.UpdateAccountAsync(destination);
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

        var fixture = new Fixture();
        var data = fixture.Build<RegistryAccountUpdatedEventData>()
            .With(x => x.AccountNafIdentifier, "1")
            .With(x => x.AccountStaffSize, "1")
            .With(x => x.Turnover, "0.1")
            .With(x => x.DeploymentStatus, "1")
            .Create();

        using var context = new AccountContext(options);
        var repository = new RegistryAccountEventRepository(context);
        _accountEntity.AccountGlobalUniqueId = data.AccountGlobalUniqueIdentifier;
        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveAccountAsync(data.AccountGlobalUniqueIdentifier);
        var removedAccount = await context.AccountEntity.FirstOrDefaultAsync();
        var removedAccountDetail = removedAccount!.MapToAccountDetail();

        // Assert
        Assert.NotNull(removedAccount);
        Assert.Equal((int)DeploymentStatus.Revoked, removedAccountDetail!.Deployment!.Status);
    }
}
