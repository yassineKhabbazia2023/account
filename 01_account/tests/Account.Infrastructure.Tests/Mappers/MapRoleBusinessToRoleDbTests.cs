// <copyright file="MapRoleBusinessToRoleDbTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Mappers;
using RoleModel = Pulse.Account.Core.Models.Role;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class MapRoleBusinessToRoleDbTests
{
    private readonly Fixture _fixture;

    public MapRoleBusinessToRoleDbTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void MapRoleRequestToRoleDb_WithNullRole_ReturnsNull()
    {
        // Arrange
        CreateRoleRequest? role = null;

        // Act
        var result = role?.MapRoleToRoleDb();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapContactEntityToSignatory_CaseSuccess()
    {
        // Arrange
        var role = _fixture.Create<CreateRoleRequest?>();

        // Act
        var result = role.MapRoleToRoleDb();

        // Assert
        Assert.Equal(role.ContactId, result.ContactId);
        Assert.Equal(role.AccountId, result.AccountId);
        Assert.Equal(role.IsFavorite, result.IsFavorite);
        Assert.Equal(role.IsSignatory, result.IsSignatory);
        Assert.Equal(role.IsDelegation, result.IsDelegation);
        Assert.False(role.IsCustomerRelation);
    }

    [Fact]
    public void MapRolesToRolesDb_ShouldMapRolesToRoleEntities()
    {
        var roles = _fixture.CreateMany<RoleModel>();

        var results = roles.MapRolesToRolesDb();

        Assert.NotNull(results);
        Assert.Equal(roles.Count(), results.Count());

        for (int i = 0; i < roles.Count(); i++)
        {
            var role = roles.ElementAt(i);
            var result = results.ElementAt(i);

            Assert.Equal(role.ContactId, result.ContactId);
            Assert.Equal(role.AccountId, result.AccountId);
            Assert.Equal(role.IsSignatory, result.IsSignatory);
            Assert.Equal(role.IsFavorite, result.IsFavorite);
            Assert.Equal(role.IsDelegation, result.IsDelegation);
        }
    }

    [Fact]
    public void MapRolesToRolesDb_WithNullSource_ShouldReturnEmptyList()
    {
        var result = MapRoleBusinessToRoleDb.MapRolesToRolesDb(null);
        Assert.Empty(result);
    }

    [Fact]
    public void MapRolesToRoleDb_WithNullSource_ShouldReturnEmptyList()
    {
        // Arrange
        IEnumerable<CreateRoleRequest> roles = null;

        // Act
        var result = roles.MapRolesToRoleDb();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void MapRolesToRoleDb_WithValidRoles_ShouldMapCorrectly()
    {
        // Arrange
        var roles = _fixture.CreateMany<CreateRoleRequest>(3).ToList();

        // Act
        var result = roles.MapRolesToRoleDb();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roles.Count, result.Count());
        for (int i = 0; i < roles.Count; i++)
        {
            var role = roles[i];
            var mappedRole = result.ElementAt(i);
            Assert.Equal(role.AccountId, mappedRole.AccountId);
            Assert.Equal(role.ContactId, mappedRole.ContactId);
            Assert.Equal(role.IsFavorite, mappedRole.IsFavorite);
            Assert.Equal(role.IsSignatory, mappedRole.IsSignatory);
            Assert.Equal(role.IsDelegation, mappedRole.IsDelegation);
        }
    }

    [Fact]
    public void MapRoleToRoleDb_WithNullRole_ReturnsNull()
    {
        // Arrange
        RoleModel role = null;

        // Act
        var result = role.MapRoleToRoleDb();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapRoleToRoleDb_WithValidRole_ShouldMapCorrectly()
    {
        // Arrange
        var role = _fixture.Create<RoleModel>();

        // Act
        var result = role.MapRoleToRoleDb();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(role.AccountId, result.AccountId);
        Assert.Equal(role.ContactId, result.ContactId);
        Assert.Equal(role.IsFavorite, result.IsFavorite);
        Assert.Equal(role.IsSignatory, result.IsSignatory);
        Assert.Equal(role.IsDelegation, result.IsDelegation);
        Assert.Equal(role.IsCustomerRelation, result.IsCustomerRelation);
    }

    [Fact]
    public void MapRolesToRolesDb_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var roles = new List<RoleModel>();

        // Act
        var result = roles.MapRolesToRolesDb();

        // Assert
        Assert.Empty(result);
    }
}
