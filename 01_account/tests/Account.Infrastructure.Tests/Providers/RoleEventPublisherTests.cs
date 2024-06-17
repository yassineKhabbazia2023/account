// <copyright file="RoleEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers
{
    public class RoleEventPublisherTests
    {
        [Fact]
        public async Task PublishRoleCreatedEventAsync_Should_PublishEvent()
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

            var contactRepository = new Mock<IContactRepository>();
            contactRepository.Setup(repository => repository.GetContactAsync(It.IsAny<int>()))
                .ReturnsAsync(contact);

            var publisherMock = new Mock<IEventPublisher>();
            var roleEventPublisher = new RoleEventPublisher(publisherMock.Object, contactRepository.Object, null);
            var request = new CreateRoleRequest
            {
                ContactId = 1,
                AccountId = 1,
                IsDelegation = false,
                IsFavorite = true,
                IsSignatory = true,
            };

            // Act
            await roleEventPublisher.PublishRoleCreatedEventAsync(request);

            // Assert
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleCreatedEventData>>(), null!, null), Times.Once);
        }

        [Fact]
        public async Task PublishRoleDeletedEventAsync_Should_PublishEvent()
        {
            // Arrange
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
            contactRepository.Setup(repository => repository.GetContactAsync(It.IsAny<int>()))
                .ReturnsAsync(contact);
            var accountRepository = new Mock<IAccountRepository>();
            accountRepository.Setup(repo => repo.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(account);

            var publisherMock = new Mock<IEventPublisher>();
            var roleEventPublisher = new RoleEventPublisher(publisherMock.Object, contactRepository.Object, accountRepository.Object);

            // Act
            await roleEventPublisher.PublishRoleDeletedEventAsync(1, 1);

            // Assert
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleDeletedEventData>>(), null!, null), Times.Once);
        }
    }
}
