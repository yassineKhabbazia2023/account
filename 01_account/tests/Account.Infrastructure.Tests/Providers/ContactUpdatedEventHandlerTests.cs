// <copyright file="ContactUpdatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactUpdatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();
        repositoryAccountMock.Setup(repository => repository.GetAccountBySignatory(It.IsAny<int>())).Returns(new List<AccountEntity>()).Verifiable();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryContactMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, null!);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, null!);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotUpdateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryMock.Object, null!);
        var message = "{\"EventType\":\"ContactUpdatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>(MockBehavior.Strict);
        var repositoryAccountMock = new Mock<IAccountEventRepository>(MockBehavior.Strict);
        repositoryContactMock.Setup(repository => repository.GetContactById(123)).Returns(new ContactEntity()
        {
            ContactId = 123,
            Status = "Connected"
        }).Verifiable();

        var accountEntityMock = new AccountEntity()
        {
            AccountId = 1,
            LegalName = "test"
        };
        repositoryContactMock.Setup(repository => repository.UpdateContactAsync(It.IsAny<ContactEntity>())).Returns(Task.CompletedTask).Verifiable();
        repositoryAccountMock.Setup(repository => repository.GetAccountBySignatory(It.IsAny<int>())).Returns(new List<AccountEntity>() { accountEntityMock }).Verifiable();
        repositoryAccountMock.Setup(repository => repository.UpdateAccountStatusByContactAsync(new List<int>() { 1 }, 3)).ReturnsAsync(new List<int>()).Verifiable();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryAccountMock.Verify(repo => repo.GetAccountBySignatory(It.IsAny<int>()), Times.Once);
        repositoryContactMock.Verify(repo => repo.GetContactById(123), Times.Once);
        repositoryAccountMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(new List<int>() { 1 }, 3), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvitedContactStatus_ShouldUpdateAccountStatusToInProgress()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();

        repositoryContactMock.Setup(repo => repo.GetContactById(123)).Returns(new ContactEntity
        {
            ContactId = 123,
            Status = "Invited"
        });

        repositoryAccountMock.Setup(repo => repo.GetAccountBySignatory(123)).Returns(new List<AccountEntity>
        {
            new AccountEntity { AccountId = 1 }
        });

        repositoryAccountMock.Setup(repo => repo.UpdateAccountStatusByContactAsync(It.IsAny<IEnumerable<int>>(), (int)DeploymentStatus.InProgress))
            .ReturnsAsync(new List<int> { 1 });

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryAccountMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(It.Is<IEnumerable<int>>(ids => ids.Single() == 1), (int)DeploymentStatus.InProgress), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithConnectedContactStatus_ShouldUpdateAccountStatusToConnected()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();

        repositoryContactMock.Setup(repo => repo.GetContactById(123)).Returns(new ContactEntity
        {
            ContactId = 123,
            Status = "Connected"
        });

        repositoryAccountMock.Setup(repo => repo.GetAccountBySignatory(123)).Returns(new List<AccountEntity>
        {
            new AccountEntity { AccountId = 1 }
        });

        repositoryAccountMock.Setup(repo => repo.UpdateAccountStatusByContactAsync(It.IsAny<IEnumerable<int>>(), (int)DeploymentStatus.Connected))
            .ReturnsAsync(new List<int> { 1 });

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryAccountMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(It.Is<IEnumerable<int>>(ids => ids.Single() == 1), (int)DeploymentStatus.Connected), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithOtherContactStatus_ShouldUpdateAccountStatusToToDeploy()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();

        repositoryContactMock.Setup(repo => repo.GetContactById(123)).Returns(new ContactEntity
        {
            ContactId = 123,
            Status = "OtherStatus"
        });

        repositoryAccountMock.Setup(repo => repo.GetAccountBySignatory(123)).Returns(new List<AccountEntity>
        {
            new AccountEntity { AccountId = 1 }
        });

        repositoryAccountMock.Setup(repo => repo.UpdateAccountStatusByContactAsync(It.IsAny<IEnumerable<int>>(), (int)DeploymentStatus.ToDeploy))
            .ReturnsAsync(new List<int> { 1 });

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryAccountMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(It.Is<IEnumerable<int>>(ids => ids.Single() == 1), (int)DeploymentStatus.ToDeploy), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoAssociatedAccounts_ShouldNotUpdateAccountStatus()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();

        repositoryContactMock.Setup(repo => repo.GetContactById(123)).Returns(new ContactEntity
        {
            ContactId = 123,
            Status = "Connected"
        });

        repositoryAccountMock.Setup(repo => repo.GetAccountBySignatory(123)).Returns(new List<AccountEntity>());

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryAccountMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ShouldNotUpdateContactOrAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = "InvalidJson";

        // Act
        var action = async () => await handler.HandleAsync(message);

        // Assert
        await action.Should().ThrowAsync<Newtonsoft.Json.JsonReaderException>();
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMessage_ShouldNotUpdateContactOrAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedEventHandler>>();
        var repositoryContactMock = new Mock<IContactEventRepository>();
        var repositoryAccountMock = new Mock<IAccountEventRepository>();

        var handler = new ContactUpdatedEventHandler(loggerMock.Object, repositoryContactMock.Object, repositoryAccountMock.Object);
        var message = string.Empty;

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryContactMock.Verify(repo => repo.UpdateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
        repositoryAccountMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Never);
    }
}
