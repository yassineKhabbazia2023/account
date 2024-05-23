// <copyright file="ContactUpdatedAccountEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactUpdatedAccountEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldUpdateAccount()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactUpdatedAccountEventHandler>>();
        var repositoryMock = new Mock<IAccountEventRepository>(MockBehavior.Strict);
        repositoryMock.Setup(repository => repository.GetContactById(123)).Returns(new ContactEntity()
        {
            ContactId = 123
        }).Verifiable();
        repositoryMock.Setup(repository => repository.GetAccountQueryByContactId(It.IsAny<int>())).Returns(new List<AccountEntity>()).Verifiable();
        repositoryMock.Setup(repository => repository.UpdateAccountStatusByContactAsync(new List<int>(), 1)).ReturnsAsync(new List<int>()).Verifiable();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new ContactUpdatedAccountEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"ContactUpdatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.UpdateAccountStatusByContactAsync(new List<int>(), 1), Times.Once);
    }
}
