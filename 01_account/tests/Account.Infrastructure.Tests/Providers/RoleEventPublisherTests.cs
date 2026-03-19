// <copyright file="RoleEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
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
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Fixture _fixture;

    public RoleEventPublisherTests()
    {
        _contactEventRepository = new Mock<IContactEventRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _eventPublisher = new Mock<IEventPublisher>();
        _roleRepository = new Mock<IRoleRepository>();

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task PublishRoleCreatedEventAsync_WithNullCreateRoleRequest_Should_Return()
    {
        CreateRoleRequest request = null!;

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, null!, null!, null!);

        await roleEventPublisher.PublishRoleCreatedEventAsync(request);

        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishRoleCreatedEventAsync_Nominal()
    {
        var request = _fixture.Create<CreateRoleRequest>();

        _contactEventRepository.Setup(c => c.GetContactAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(a => a.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<AccountDetail>());

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, _contactEventRepository.Object, _accountRepository.Object, null!);

        await roleEventPublisher.PublishRoleCreatedEventAsync(request);

        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<RoleCreatedEventData>>(), null!, null), Times.Once);
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
            Phone = new List<Phone> { new Phone { PhoneNumber = "0600000000" } },
            AccountId = 1,
            Legal = new Legal
            {
                LegalName = "jhonny pizza", Siren = "123456789"
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
            _accountRepository.Object,
            _roleRepository.Object);

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

        _accountRepository.Setup(a => a.GetAccountAsync(accountId)).ReturnsAsync(_fixture.Create<AccountDetail>());

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, _contactEventRepository.Object, _accountRepository.Object, _roleRepository.Object);

        // act
        await roleEventPublisher.PublishRoleUpdatedEventAsync(accountId, contactId, isSignatory);

        // arrange
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<RoleUpdatedEvent>(), null!, null!), Times.Once);
    }

    [Fact]
    public async Task PublishRoleFavoriteStatusChangedEventAsync_Should_Include_IsSignatory()
    {
        // arrange
        int accountId = 1;
        int contactId = 2;
        bool isFavorite = true;

        var existingRole = new Core.Models.Role
        {
            AccountId = accountId,
            ContactId = contactId,
            IsSignatory = true, // Le rôle existant a IsSignatory = true
            IsFavorite = false,
            IsDelegation = false,
            IsCustomerRelation = true
        };

        _roleRepository.Setup(r => r.GetContactRoleAsync(accountId, contactId))
            .ReturnsAsync(existingRole);
        _accountRepository.Setup(a => a.GetAccountAsync(accountId)).ReturnsAsync(_fixture.Create<AccountDetail>());

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, _contactEventRepository.Object, _accountRepository.Object, _roleRepository.Object);

        // act
        await roleEventPublisher.PublishRoleFavoriteStatusChangedEventAsync(accountId, contactId, isFavorite);

        // assert
        _eventPublisher.Verify(x => x.PublishAsync(
            It.Is<RoleUpdatedEvent>(e =>
                e.Data.AccountId == accountId &&
                e.Data.ContactId == contactId &&
                e.Data.IsFavorite == isFavorite &&
                e.Data.IsSignatory == true && // Vérifier que IsSignatory est préservé
                e.Data.IsCustomerRelation == true
            ), null!, null!), Times.Once);
    }

    [Fact]
    public async Task PublishRoleFavoriteStatusChangedEventAsync_WithNonExistentRole_Should_ThrowException()
    {
        // arrange
        int accountId = 1;
        int contactId = 2;
        bool isFavorite = true;


        _roleRepository.Setup(r => r.GetContactRoleAsync(accountId, contactId))
            .ReturnsAsync((Core.Models.Role?)null);

        var roleEventPublisher = new RoleEventPublisher(_eventPublisher.Object, _contactEventRepository.Object, _accountRepository.Object, _roleRepository.Object);

        // act & assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => roleEventPublisher.PublishRoleFavoriteStatusChangedEventAsync(accountId, contactId, isFavorite));

        Assert.Contains($"Role not found for AccountId: {accountId}, ContactId: {contactId}", exception.Message);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<RoleUpdatedEvent>(), null!, null!), Times.Never);
    }
}
