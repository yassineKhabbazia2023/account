// <copyright file="MapToRoleEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Mappers.Events;

public class MapToRoleEntityTests
{
    private readonly Fixture _fixture;

    public MapToRoleEntityTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void ToRoleEntity_MapsCorrectly()
    {
        // Arrange
        RegistryRoleCreatedEventData eventData = new RegistryRoleCreatedEventData
        {
            AccountId = 1,
            AccountNumber = "1289090",
            ContactId = 2,
            Email = "email@test.fr",
            IsFavorite = true,
            RoleSignatory = false,
            IsCustomerRelation = true,
            ContactFlagPortailFactures = true
        };

        // Act
        var result = eventData.ToRoleEntity(null, null, true);

        // Assert
        Assert.Equal(eventData.ContactId, result.ContactId);
        Assert.Equal(eventData.AccountId, result.AccountId);
        Assert.Equal(eventData.IsFavorite, result.IsFavorite);
        Assert.Equal(eventData.RoleSignatory, result.IsSignatory);
        Assert.Equal(eventData.IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(eventData.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
    }

    [Fact]
    public void ToRoleEntity_MapsCorrectly_WithOverriddenValues()
    {
        // Arrange
        var expectedContactId = 25;
        var expectedAccountId = 602;
        var eventData = new RegistryRoleCreatedEventData
        {
            AccountId = 1, // Cette valeur sera surchargée par expectedAccountId
            AccountNumber = "1289090",
            ContactId = 2, // Cette valeur sera surchargée par expectedContactId
            Email = "email@test.fr",
            IsFavorite = true,
            RoleSignatory = false,
            IsCustomerRelation = true,
            ContactFlagPortailFactures = true
        };

        // Act
        var result = eventData.ToRoleEntity(expectedAccountId, expectedContactId, true);

        // Assert
        Assert.Equal(expectedContactId, result.ContactId);
        Assert.Equal(expectedAccountId, result.AccountId);
        Assert.Equal(eventData.IsFavorite, result.IsFavorite);
        Assert.Equal(eventData.RoleSignatory, result.IsSignatory);
        Assert.Equal(eventData.IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(eventData.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
    }

    [Fact]
    public void ToRoleEntity_WithNullRegistryRoleCreatedEventData_ShouldReturnNull()
    {
        RegistryRoleCreatedEventData data = null!;
        var result = MapToRoleEntity.ToRoleEntity(data, null, null, false);
        Assert.Null(result);
    }

    [Fact]
    public void ToCreateRoleRequest_MapsCorrectly()
    {
        // Arrange
        var roleEntity = new RoleEntity
        {
            ContactId = 100,
            Contact = new ContactEntity
            {
                ContactId = 100,
                ContactGlobalUniqueId = Guid.NewGuid(),
            },
            AccountId = 122,
            Account = new AccountEntity
            {
                AccountId = 122,
                AccountGlobalUniqueId = Guid.NewGuid(),
            },
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true,
            IsCustomerRelation = true,
            ContactFlagPortailFactures = true,
        };

        // Act
        var createdRole = roleEntity.ToCreateRoleRequest();

        // Assert
        Assert.Equal(roleEntity.ContactId, createdRole.ContactId);
        Assert.Equal(roleEntity.AccountId, createdRole.AccountId);
        Assert.Equal(roleEntity.IsFavorite, createdRole.IsFavorite);
        Assert.Equal(roleEntity.IsSignatory, createdRole.IsSignatory);
        Assert.Equal(roleEntity.IsCustomerRelation, createdRole.IsCustomerRelation);
        Assert.Equal(roleEntity.ContactFlagPortailFactures, createdRole.ContactFlagPortailFactures);
        Assert.Equal(roleEntity.Contact.ContactGlobalUniqueId, createdRole.ContactGlobalUniqueId);
        Assert.Equal(roleEntity.Account.AccountGlobalUniqueId, createdRole.AccountGlobalUniqueId);
    }

    [Fact]
    public void ToRoleEntity_Should_MapCreateRoleRequestToRoleEntity()
    {
        var request = _fixture.Create<CreateRoleRequest>();

        var result = request.ToRoleEntity();

        Assert.NotNull(result);
        Assert.Equal(request.ContactId, result.ContactId);
        Assert.Equal(request.AccountId, result.AccountId);
        Assert.Equal(request.IsSignatory, result.IsSignatory);
        Assert.Equal(request.IsDelegation, result.IsDelegation);
        Assert.Equal(request.IsFavorite, result.IsFavorite);
        Assert.Equal(request.IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(request.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
    }

    [Fact]
    public void ToRoleEntity_WithNullRequest_Should_ReturnNull()
    {
        CreateRoleRequest request = null!;
        var result = MapToRoleEntity.ToRoleEntity(request);
        Assert.Null(result);
    }

    [Fact]
    public void ToRoleEntities_Should_MapCreateRoleRequestsToRoleEntities()
    {
        var requests = _fixture.CreateMany<CreateRoleRequest>();

        var results = requests.ToRoleEntities();

        Assert.NotNull(results);
        Assert.Equal(requests.Count(), results.Count());

        for (int i = 0; i < requests.Count(); i++)
        {
            var request = requests.ElementAt(i);
            var result = results.ElementAt(i);

            Assert.Equal(request.ContactId, result.ContactId);
            Assert.Equal(request.AccountId, result.AccountId);
            Assert.Equal(request.IsSignatory, result.IsSignatory);
            Assert.Equal(request.IsDelegation, result.IsDelegation);
            Assert.Equal(request.IsFavorite, result.IsFavorite);
            Assert.Equal(request.IsCustomerRelation, result.IsCustomerRelation);
            Assert.Equal(request.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
        }
    }

    [Fact]
    public void ToRoleEntities_WithNullRequest_Should_ReturnEmptyList()
    {
        var results = MapToRoleEntity.ToRoleEntities(null!);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void ToRole_Should_MapRoleCreatedEventDataToRole()
    {
        var source = _fixture.Create<RoleCreatedEventData>();

        var result = source.ToRole();

        Assert.NotNull(result);
        Assert.Equal(source.ContactId, result.ContactId);
        Assert.Equal(source.AccountId, result.AccountId);
        Assert.Equal(source.IsSignatory, result.IsSignatory);
        Assert.Equal(source.IsDelegation, result.IsDelegation);
        Assert.Equal(source.IsFavorite, result.IsFavorite);
        Assert.Equal(source.IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(source.ContactFlagPortailFactures, result.ContactFlagPortailFactures);
    }

    [Fact]
    public void ToRole_WithNullSource_Should_ReturnNull()
    {
        var result = MapToRoleEntity.ToRole(null!);
        Assert.Null(result);
    }

    [Fact]
    public void ToCreateRoleRequests_Should_MapRoleEntityListToCreateRoleRequests()
    {
        var source = _fixture.CreateMany<RoleEntity>(3);
        var delegatorId = 8;
        var includePennylaneAccess = false;
        var result = source.ToCreateRoleRequests(delegatorId, includePennylaneAccess);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count());

        for (var i = 0; i < source.Count(); i++)
        {
            var sourceItem = source.ElementAt(i);
            var resultItem = result.ElementAt(i);

            Assert.Equal(delegatorId, resultItem.DelegatorId);
            Assert.Equal(includePennylaneAccess, resultItem.IncludePennylaneAccess);
        }
    }

    [Fact]
    public void ToCreateRoleRequests_WithNullSource_Should_ReturnEmptyList()
    {
        var result = MapToRoleEntity.ToCreateRoleRequests(null!, 1, false);

        Assert.Empty(result);
    }

    [Fact]
    public void ToRoleEntity_WithIsCustomerRelationTrue_Should_SetActionLevelToDirectClientRelation()
    {
        // Arrange
        var eventData = new RegistryRoleCreatedEventData
        {
            AccountId = 1,
            AccountNumber = "ACC001",
            ContactId = 2,
            Email = "test@email.fr",
            IsCustomerRelation = true,
        };

        // Act
        var result = eventData.ToRoleEntity(null, null, true);

        // Assert
        Assert.Equal((int)ActionLevelType.DirectClientRelation, result.ActionLevel);
    }

    [Fact]
    public void ToRoleEntity_WithIsCustomerRelationFalse_Should_SetActionLevelToNotAssigned()
    {
        // Arrange
        var eventData = new RegistryRoleCreatedEventData
        {
            AccountId = 1,
            AccountNumber = "ACC001",
            ContactId = 2,
            Email = "test@email.fr",
            IsCustomerRelation = false,
        };

        // Act
        var result = eventData.ToRoleEntity(null, null, false);

        // Assert
        Assert.Equal((int)ActionLevelType.NotAssigned, result.ActionLevel);
    }

    [Fact]
    public void ToRoleEntity_WithIsCustomerRelationNull_Should_SetActionLevelToNotAssigned()
    {
        // Arrange
        var eventData = new RegistryRoleCreatedEventData
        {
            AccountId = 1,
            AccountNumber = "ACC001",
            ContactId = 2,
            Email = "test@email.fr",
        };

        // Act
        var result = eventData.ToRoleEntity(null, null, null);

        // Assert
        Assert.Equal((int)ActionLevelType.NotAssigned, result.ActionLevel);
    }
}
