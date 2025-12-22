// <copyright file="RegistryAccountEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

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
        IsActive = true
    };

    [Fact]
    public async Task RemoveAccountAsync_ShouldThrowNotFoundException_IfAccountIdDoesNotExists()
    {
        Guid accountGUI = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using (var context = new AccountContext(options))
        {
            var action = async () => await new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance).RemoveAccountAsync(accountGUI);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }

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
        var repository = new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance);

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
        var repository = new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance);
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
        var repository = new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance);
        _accountEntity.AccountGlobalUniqueId = data.AccountGlobalUniqueIdentifier;
        await context.AccountEntity.AddAsync(_accountEntity);
        await context.SaveChangesAsync();

        // Act
        await repository.RemoveAccountAsync(data.AccountGlobalUniqueIdentifier);
        var removedAccount = await context.AccountEntity.IgnoreQueryFilters().FirstOrDefaultAsync();
        var removedAccountDetail = removedAccount!.MapToAccountDetail();

        Assert.NotNull(removedAccount);
        removedAccountDetail.IsActive.Should().BeFalse();
        // Assert
    }

    [Fact]
    public async Task UpdateAccountAsync_ShouldThrowNotFoundException_IfGlobalIdDoesNotExistsInDB()
    {
        // arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;

        var eventData = new RegistryAccountUpdatedEventData();

        using var context = new AccountContext(options);
        AccountEntity account = new AccountEntity()
        {
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "1234456",
            LegalName = "Marc Company",
            CreatedBy = "Me"
        };
        context.AccountEntity.Add(account);
        context.SaveChanges();
        var repository = new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance);

        // act
        var action = async () => await repository.UpdateAccountAsync(eventData);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DoesAccountExist_WithExistingAccount_ShouldReturnTrue()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;
        using var context = new AccountContext(options);

        var account = new AccountEntity()
        {
            AccountId = 2,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = "number",
            LegalName = "legal",
            Email = "email@kpmg.fr",
            CreatedBy = "moi",
            IsActive = true
        };
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var repository = new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance);

        var result = await repository.DoesAccountExistAsync(account.AccountGlobalUniqueId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesAccountExist_WithNoExistingAccount_ShouldReturnFalse()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;
        using var context = new AccountContext(options);

        var repository = new RegistryAccountEventRepository(context, NullLogger<RegistryAccountEventRepository>.Instance);

        var result = await repository.DoesAccountExistAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }
}
