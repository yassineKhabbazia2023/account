// <copyright file="RoleEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RoleEventPublisherTests
{
    private readonly Mock<IContactEventRepository> _contactEventRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IEventPublisher> _eventPublisher;

    public RoleEventPublisherTests()
    {
        _contactEventRepository = new Mock<IContactEventRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _eventPublisher = new Mock<IEventPublisher>();
    }

    [Fact]
    public async Task PublishRoleCreatedEventAsync_WithNullCreateRoleRequest_Should_Return()
    {
        CreateRoleRequest request = null!;

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, null!, null!);

        await roleEventPublisher.PublishRoleCreatedEventAsync(request);

        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishRoleDeletedEventAsync_Should_PublishEvent()
    {
        // Arrange
        var contact = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PersonaName = "Collab GS",
            Status = ContactStatus.Declared.ToString(),
            Type = "collaborator",
            CreationDate = DateTime.Parse("2024-04-16T09:19:16Z"),
            ContactGlobalUniqueId = Guid.NewGuid(),
            RoleEntity = new List<RoleEntity>()
            {
                new()
                {
                    AccountId = 1,
                    Account = new AccountEntity
                    {
                        AccountId = 1,
                        LegalName = "jhonny pizza",
                        AccountNumber = "19999999",
                        Email = "jhonny@test.com",
                        CreatedBy = "test@test.com",
                        AccountGlobalUniqueId = Guid.NewGuid(),
                    },
                }
            },
        };

        var account = new AccountDetail
        {
            AccountId = 1,
            Legal = new Legal
            {
                LegalName = "jhonny pizza"
            },
            AccountNumber = "19999999",
            Email = "jhonny@test.com",
            AccountGlobalUniqueId = Guid.NewGuid(),
        };

        _contactEventRepository.Setup(r => r.GetContactAsync(It.IsAny<int>(), true))
            .ReturnsAsync(contact);
        _accountRepository.Setup(r => r.GetAccountAsync(It.IsAny<int>()))
            .ReturnsAsync(account);

        var roleEventPublisher = new RoleEventPublisher(
            _eventPublisher.Object,
            _contactEventRepository.Object,
            _accountRepository.Object);

        // Act
        await roleEventPublisher.PublishRoleDeletedEventAsync(1, 1);

        // Assert
        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleDeletedEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishRoleUpdatedEvent_ShouldExecuteCorrectly()
    {
        // arrange
        int accountId = 1;
        int contactId = 2;
        bool isSignatory = false;

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, _contactEventRepository.Object, _accountRepository.Object);

        // act
        await roleEventPublisher.PublishRoleUpdatedEventAsync(accountId, contactId, isSignatory);

        // arrange
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<RoleUpdatedEvent>(), null!, null!), Times.Once);
    }

    [Fact]
    public async Task PublishRoleFavoriteStatusChangedEventAsync()
    {
        // arrange
        int accountId = 1;
        int contactId = 2;
        bool isFavorite = true;

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, _contactEventRepository.Object, _accountRepository.Object);

        // act
        await roleEventPublisher.PublishRoleFavoriteStatusChangedEventAsync(accountId, contactId, isFavorite);

        // arrange
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<RoleUpdatedEvent>(), null!, null!), Times.Once);
    }
}
