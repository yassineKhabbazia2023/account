// <copyright file="RoleEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RoleEventPublisherTests
{

    [Fact]
    public async Task PublishRoleCreatedEventAsync_WithNullCreateRoleRequest_Should_Return()
    {
        CreateRoleRequest request = null!;

        var publisherMock = new Mock<IEventPublisher>();
        var scopeMock = new Mock<IServiceScopeFactory>();
        var roleEventPublisher = new RoleEventPublisher(publisherMock.Object, null!, null!, scopeMock.Object);

        await roleEventPublisher.PublishRoleCreatedEventAsync(request);

        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleCreatedEventData>>(), null!, null), Times.Never);
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
                new RoleEntity
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

        var contactRepository = new Mock<IContactRepository>();
        contactRepository.Setup(repository => repository.GetContactAsync(It.IsAny<int>(), true))
            .ReturnsAsync(contact);
        var accountRepository = new Mock<IAccountRepository>();
        accountRepository.Setup(repo => repo.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(account);

        var publisherMock = new Mock<IEventPublisher>();
        var scopeMock = new Mock<IServiceScopeFactory>();
        var roleEventPublisher = new RoleEventPublisher(publisherMock.Object, contactRepository.Object, accountRepository.Object, scopeMock.Object);

        // Act
        await roleEventPublisher.PublishRoleDeletedEventAsync(1, 1);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleDeletedEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishRoleUpdatedEvent_ShouldExecuteCorrectly()
    {
        // arrange
        int accountId = 1;
        int contactId = 2;
        bool isSignatory = false;

        var contactRepository = new Mock<IContactRepository>();
        var accountRepository = new Mock<IAccountRepository>();
        var publisherMock = new Mock<IEventPublisher>();
        var scopeMock = new Mock<IServiceScopeFactory>();
        var roleEventPublisher = new RoleEventPublisher(publisherMock.Object, contactRepository.Object, accountRepository.Object, scopeMock.Object);

        // act
        await roleEventPublisher.PublishRoleUpdatedEventAsync(accountId, contactId, isSignatory);

        // arrange
        publisherMock.Verify(x => x.PublishAsync(It.IsAny<RoleUpdatedEvent>(), null!, null!), Times.Once);
    }

}
