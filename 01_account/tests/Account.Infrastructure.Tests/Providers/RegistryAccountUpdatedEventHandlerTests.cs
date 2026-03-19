// <copyright file="RegistryAccountUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RegistryAccountUpdatedEventHandlerTests
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
    public async Task HandleAsync_WithValidMessage_ShouldUpdateAccount_And_PublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.GetAccountByGuidAsync(It.IsAny<Guid>()))
            .ReturnsAsync((AccountDetail?)null);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + Guid.NewGuid().ToString() + "\",\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Once);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingAccountId_ShouldNotUpdateAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>())) !
        .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"LegalName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateAccount_And_NotPublishEvent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenStaffSizeRangeCleared_ShouldPreserveExistingValue()
    {
        // Arrange
        var accountGuid = Guid.NewGuid();
        var currentAccount = new AccountDetail
        {
            AccountId = 1,
            AccountNumber = "123",
            Legal = new Legal { LegalName = "Test", Siren = "123456789", StaffSizeRange = "10-50" },
            Phone = new List<Phone>()
        };

        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        repositoryMock.Setup(r => r.GetAccountByGuidAsync(accountGuid))
            .ReturnsAsync(currentAccount);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()))
            .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + accountGuid + "\",\"LegalName\":\"John Doe\",\"AccountStaffSizeSlice\":null}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.Is<RegistryAccountUpdatedEventData>(
            e => e.AccountStaffSizeSlice == "10-50")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountingTypeCleared_ShouldPreserveExistingValue()
    {
        // Arrange
        var accountGuid = Guid.NewGuid();
        var currentAccount = new AccountDetail
        {
            AccountId = 1,
            AccountNumber = "123",
            Legal = new Legal { LegalName = "Test", Siren = "123456789" },
            Accounting = new Accounting { AccountingType = "Engagement" },
            Phone = new List<Phone>()
        };

        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        repositoryMock.Setup(r => r.GetAccountByGuidAsync(accountGuid))
            .ReturnsAsync(currentAccount);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()))
            .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + accountGuid + "\",\"LegalName\":\"John Doe\",\"AccountTypeTenueComptable\":null}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.Is<RegistryAccountUpdatedEventData>(
            e => e.AccountTypeTenueComptable == "Engagement")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenBothFieldsCleared_ShouldPreserveBothExistingValues()
    {
        // Arrange
        var accountGuid = Guid.NewGuid();
        var currentAccount = new AccountDetail
        {
            AccountId = 1,
            AccountNumber = "123",
            Legal = new Legal { LegalName = "Test", Siren = "123456789", StaffSizeRange = "10-50" },
            Accounting = new Accounting { AccountingType = "Engagement" },
            Phone = new List<Phone>()
        };

        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        repositoryMock.Setup(r => r.GetAccountByGuidAsync(accountGuid))
            .ReturnsAsync(currentAccount);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()))
            .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + accountGuid + "\",\"LegalName\":\"John Doe\",\"AccountStaffSizeSlice\":\"\",\"AccountTypeTenueComptable\":\"\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.Is<RegistryAccountUpdatedEventData>(
            e => e.AccountStaffSizeSlice == "10-50" && e.AccountTypeTenueComptable == "Engagement")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNewValuesProvided_ShouldUseNewValues()
    {
        // Arrange
        var accountGuid = Guid.NewGuid();
        var currentAccount = new AccountDetail
        {
            AccountId = 1,
            AccountNumber = "123",
            Legal = new Legal { LegalName = "Test", Siren = "123456789", StaffSizeRange = "10-50" },
            Accounting = new Accounting { AccountingType = "Engagement" },
            Phone = new List<Phone>()
        };

        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        repositoryMock.Setup(r => r.GetAccountByGuidAsync(accountGuid))
            .ReturnsAsync(currentAccount);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()))
            .ReturnsAsync(_accountEntity.MapToAccountDetail());

        var publisherMock = new Mock<IAccountEventPublisher>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\": \"" + accountGuid + "\",\"LegalName\":\"John Doe\",\"AccountStaffSizeSlice\":\"50-100\",\"AccountTypeTenueComptable\":\"Tresorerie\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.Is<RegistryAccountUpdatedEventData>(
            e => e.AccountStaffSizeSlice == "50-100" && e.AccountTypeTenueComptable == "Tresorerie")), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmptyMessage_ShouldNotProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync("");

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenWhitespaceMessage_ShouldNotProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>();
        var publisherMock = new Mock<IAccountEventPublisher>();
        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);

        // Act
        await handler.HandleAsync("   ");

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithProspectMessage_ShouldUseGenericFlow()
    {
        // Arrange
        var accountGuid = Guid.NewGuid();
        var currentAccount = new AccountDetail
        {
            AccountId = 1,
            AccountNumber = "123",
            Legal = new Legal { LegalName = "Test", Siren = "123456789", StaffSizeRange = "10-50" },
            Accounting = new Accounting { AccountingType = "Engagement" },
            Phone = new List<Phone>()
        };

        var loggerMock = new Mock<ILogger<RegistryAccountUpdatedEventHandler>>();
        var repositoryMock = new Mock<IRegistryAccountEventRepository>(MockBehavior.Strict);
        var publisherMock = new Mock<IAccountEventPublisher>(MockBehavior.Strict);

        repositoryMock.Setup(r => r.GetAccountByGuidAsync(accountGuid))
            .ReturnsAsync(currentAccount);
        repositoryMock.Setup(r => r.UpdateAccountAsync(It.IsAny<RegistryAccountUpdatedEventData>()))
            .ReturnsAsync(_accountEntity.MapToAccountDetail());
        publisherMock.Setup(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()))
            .Returns(Task.CompletedTask);

        var handler = new RegistryAccountUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, publisherMock.Object);
        var message = "{\"EventType\":\"RegistryAccountUpdatedEvent\",\"Data\":{\"AccountGlobalUniqueIdentifier\":\"" + accountGuid + "\",\"AccountType\":\"" + GlobalConstants.ProspectAccountType + "\",\"AccountStaffSizeSlice\":null,\"AccountTypeTenueComptable\":null}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.GetAccountByGuidAsync(accountGuid), Times.Once);
        repositoryMock.Verify(r => r.UpdateAccountAsync(It.Is<RegistryAccountUpdatedEventData>(
            e => e.AccountStaffSizeSlice == "10-50" && e.AccountTypeTenueComptable == "Engagement")), Times.Once);
        publisherMock.Verify(p => p.PublishAccountUpdatedEventAsync(It.IsAny<AccountDetail>()), Times.Once);
    }
}
