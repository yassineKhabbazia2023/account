// <copyright file="MapRoleBusinessToRoleDbTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

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
    public void MapRoleToRoleDb_WithNullRole_ReturnsNull()
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
        CreateRoleRequest? role = _fixture.Create<CreateRoleRequest?>();
        var expected = new RoleEntity()
        {
            ContactId = role!.ContactId,
            AccountId = role!.AccountId,
            IsFavorite = role!.IsFavorite,
            IsSignatory = role!.IsSignatory,
            IsDelegation = role!.IsDelegation,
        };

        // Act
        var result = role.MapRoleToRoleDb();

        // Assert
        Assert.Equivalent(expected, result);
    }

    [Fact]
    public void MapRolesToRolesDb_ShouldMapRolesToRoleEntities()
    {
        var roles = _fixture.CreateMany<Core.Models.Role>();

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
}
