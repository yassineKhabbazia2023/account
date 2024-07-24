// <copyright file="MapToRoleEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
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
        var source = new RegistryRoleCreatedEventData
        {
            AccountId = Guid.NewGuid(),
            AccountNumber = "1289090",
            ContactId = Guid.NewGuid(),
            Email = "email@test.fr",
            IsFavorite = true,
            RoleDelegataireEmail = "delegataire@email.fr",
            RoleSignatory = false
        };
        int accountId = 1;
        int contactId = 2;

        // Act
        var result = source.ToRoleEntity(accountId, contactId);

        // Assert
        Assert.Equal(contactId, result.ContactId);
        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(source.IsFavorite, result.IsFavorite);
        Assert.Equal(source.RoleSignatory, result.IsSignatory);
    }

    [Fact]
    public void ToRoleEntity_WithNullRegistryRoleCreatedEventData_ShouldReturnNull()
    {
        var result = MapToRoleEntity.ToRoleEntity(null!, 1, 2);
        Assert.Null(result);
    }

    [Fact]
    public void ToCreateRoleRequest_MapsCorrectly()
    {
        // Arrange
        var roleEntity = new RoleEntity
        {
            ContactId = 100,
            AccountId = 122,
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true
        };

        // Act
        var createdRole = roleEntity.ToCreateRoleRequest();

        // Assert
        Assert.Equal(createdRole.ContactId, roleEntity.ContactId);
        Assert.Equal(createdRole.AccountId, roleEntity.AccountId);
        Assert.Equal(createdRole.IsFavorite, roleEntity.IsFavorite);
        Assert.Equal(createdRole.IsSignatory, roleEntity.IsSignatory);
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
    }

    [Fact]
    public void ToRoleEntity_WithNullRequest_Should_ReturnNull()
    {
        var result = MapToRoleEntity.ToRoleEntity(null!);
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
    }

    [Fact]
    public void ToRole_WithNullSource_Should_ReturnNull()
    {
        var result = MapToRoleEntity.ToRole(null!);
        Assert.Null(result);
    }
}
