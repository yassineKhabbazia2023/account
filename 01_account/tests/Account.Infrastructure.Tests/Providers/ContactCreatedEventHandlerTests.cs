// <copyright file="ContactCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactCreatedEventHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<ContactCreatedEventHandler>> _logger;
    private readonly Mock<IContactEventRepository> _contactRepositoryMock;
    private readonly Mock<IRoleEventRepository> _roleEventRepositoryMock;
    private readonly Mock<IRoleEventPublisher> _roleEventPublisherMock;
    private readonly Mock<IAccountEventRepository> _accountRepositoryMock;
    private readonly Mock<IHistoryEventPublisher> _historyEventPublisher;

    public ContactCreatedEventHandlerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _logger = new Mock<ILogger<ContactCreatedEventHandler>>();
        _contactRepositoryMock = new Mock<IContactEventRepository>();
        _roleEventRepositoryMock = new Mock<IRoleEventRepository>();
        _roleEventPublisherMock = new Mock<IRoleEventPublisher>();
        _accountRepositoryMock = new Mock<IAccountEventRepository>();
        _historyEventPublisher = new Mock<IHistoryEventPublisher>();
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ShouldCreatesContactButNotRole()
    {
        // Arrange
        _roleEventRepositoryMock.Setup(x => x.DoesRoleExistAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        _accountRepositoryMock.Setup(x => x.GetAccountByNumberAsync(It.IsAny<string>())).ReturnsAsync(_fixture.Create<AccountEntity>());

        var handler = new ContactCreatedEventHandler(_logger.Object,
            _contactRepositoryMock.Object,
            _roleEventRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _roleEventPublisherMock.Object,
            _historyEventPublisher.Object);
        var message = "{\"EventType\":\"ContactCreatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\",\"AccountNumber\":\"11111111111\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _contactRepositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Once);
        _accountRepositoryMock.Verify(x => x.GetAccountByNumberAsync(It.IsAny<string>()), Times.Once);
        _roleEventRepositoryMock.Verify(x => x.CreateRoleForNewContactAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleEventPublisherMock.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
        _historyEventPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotCreateContact()
    {
        // Arrange
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactCreatedEventHandler(null!, repositoryMock.Object, null!, null!, null!, null!);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotCreateContact()
    {
        // Arrange
        var handler = new ContactCreatedEventHandler(_logger.Object, _contactRepositoryMock.Object, null!, null!, null!, null!);
        var message = "{\"EventType\":\"ContactCreatedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _contactRepositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotCreateContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactCreatedEventHandler>>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var handler = new ContactCreatedEventHandler(loggerMock.Object, repositoryMock.Object, null!, null!, null!, null!);
        var message = "{\"EventType\":\"ContactCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_And_AccountNumber_ShouldCreateContactAndRole()
    {
        // Arrange
        _roleEventRepositoryMock.Setup(r => r.CreateRoleForNewContactAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new CreateRoleRequest
            {
                AccountId = 1,
                ContactId = 123,
            })
            .Verifiable();
        _roleEventPublisherMock.Setup(r => r.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()))
            .Verifiable();

        _accountRepositoryMock.Setup(x => x.GetAccountByNumberAsync(It.IsAny<string>())).ReturnsAsync(_fixture.Create<AccountEntity>());

        var handler = new ContactCreatedEventHandler(_logger.Object,
            _contactRepositoryMock.Object,
            _roleEventRepositoryMock.Object,
            _accountRepositoryMock.Object,
            _roleEventPublisherMock.Object,
            _historyEventPublisher.Object);
        var message = "{\"EventType\":\"ContactCreatedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\",\"AccountNumber\":\"69696969\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        _contactRepositoryMock.Verify(repo => repo.CreateContactAsync(It.IsAny<ContactEntity>()), Times.Once);
        _accountRepositoryMock.Verify(x => x.GetAccountByNumberAsync(It.IsAny<string>()), Times.Once);
        _roleEventRepositoryMock.Verify(x => x.CreateRoleForNewContactAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _roleEventPublisherMock.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>()), Times.Once);
        _historyEventPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }
}
