// <copyright file="ContactRemovedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactRemovedEventHandlerTests
{
    private readonly Fixture _fixture;

    public ContactRemovedEventHandlerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldRemoveContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var rolePublisher = new Mock<IRoleEventPublisher>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleRepository = new Mock<IRoleEventRepository>();
        var delegationRepository = new Mock<IDelegationEventRepository>();

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        roleRepository.Setup(repo => repo.DeleteContactRolesAsync(It.IsAny<int>())).ReturnsAsync(_fixture.CreateMany<RoleEntity>());

        var handler = new ContactRemovedEventHandler(loggerMock.Object,
            rolePublisher.Object,
            repositoryMock.Object,
            roleRepository.Object,
            delegationRepository.Object);
        var message = "{\"EventType\":\"ContactRemovedEvent\",\"Data\":{\"ContactId\":123,\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        rolePublisher.Verify(publisher => publisher.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Once);
        roleRepository.Verify(repo => repo.DeleteContactRolesAsync(It.IsAny<int>()), Times.Once);
        delegationRepository.Verify(repo => repo.DeleteContactDelegationsAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_ShouldNotRevokContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var rolePublisher = new Mock<IRoleEventPublisher>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleRepository = new Mock<IRoleEventRepository>();
        var delegationRepository = new Mock<IDelegationEventRepository>();
        var handler = new ContactRemovedEventHandler(loggerMock.Object,
            rolePublisher.Object,
            repositoryMock.Object,
            roleRepository.Object,
            delegationRepository.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        rolePublisher.Verify(publisher => publisher.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Never);
        roleRepository.Verify(repo => repo.DeleteContactRolesAsync(It.IsAny<int>()), Times.Never);
        delegationRepository.Verify(repo => repo.DeleteContactDelegationsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingContactId_ShouldNotRemoveContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var rolePublisher = new Mock<IRoleEventPublisher>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleRepository = new Mock<IRoleEventRepository>();
        var delegationRepository = new Mock<IDelegationEventRepository>();
        var handler = new ContactRemovedEventHandler(loggerMock.Object,
            rolePublisher.Object,
            repositoryMock.Object,
            roleRepository.Object,
            delegationRepository.Object);
        var message = "{\"EventType\":\"ContactRemovedEvent\",\"Data\":{\"FirstName\":\"John Doe\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        rolePublisher.Verify(publisher => publisher.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Never);
        roleRepository.Verify(repo => repo.DeleteContactRolesAsync(It.IsAny<int>()), Times.Never);
        delegationRepository.Verify(repo => repo.DeleteContactDelegationsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMessageMissingData_ShouldNotRemoveContact()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ContactRemovedEventHandler>>();
        var rolePublisher = new Mock<IRoleEventPublisher>();
        var repositoryMock = new Mock<IContactEventRepository>();
        var roleRepository = new Mock<IRoleEventRepository>();
        var delegationRepository = new Mock<IDelegationEventRepository>();
        var handler = new ContactRemovedEventHandler(loggerMock.Object,
            rolePublisher.Object,
            repositoryMock.Object,
            roleRepository.Object,
            delegationRepository.Object);
        var message = "{\"EventType\":\"ContactRemovedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        rolePublisher.Verify(publisher => publisher.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        repositoryMock.Verify(repo => repo.RemoveContactAsync(It.IsAny<int>()), Times.Never);
        roleRepository.Verify(repo => repo.DeleteContactRolesAsync(It.IsAny<int>()), Times.Never);
        delegationRepository.Verify(repo => repo.DeleteContactDelegationsAsync(It.IsAny<int>()), Times.Never);
    }
}
