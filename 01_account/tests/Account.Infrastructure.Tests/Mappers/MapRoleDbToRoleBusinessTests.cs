// <copyright file="MapRoleDbToRoleBusinessTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class MapRoleDbToRoleBusinessTests
{
    private readonly Fixture _fixture;

    public MapRoleDbToRoleBusinessTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void MapToRole_ShouldMapRole()
    {
        var role = _fixture.Create<RoleEntity>();

        var result = role.MapToRole();

        Assert.NotNull(result);
        Assert.Equal(role.ContactId, result.ContactId);
        Assert.Equal(role.AccountId, result.AccountId);
        Assert.Equal(role.IsSignatory, result.IsSignatory);
        Assert.Equal(role.IsFavorite, result.IsFavorite);
        Assert.Equal(role.IsDelegation, result.IsDelegation);
    }

    [Fact]
    public void MapToRole_WithNullSource_ShouldReturnNull()
    {
        var result = MapRoleDbToRoleBusiness.MapToRole(null!);

        Assert.Null(result);
    }

    [Fact]
    public void MapToRoles_ShouldMapRoles()
    {
        var roles = _fixture.CreateMany<RoleEntity>();

        var result = roles.MapToRoles().ToList();

        Assert.NotNull(result);
        Assert.Equal(roles.Count(), result.Count);

        for (int i = 0; i < result.Count; i++)
        {
            var role = roles.ElementAt(i);
            var res = result[i];

            Assert.Equal(role.ContactId, res.ContactId);
            Assert.Equal(role.AccountId, res.AccountId);
            Assert.Equal(role.IsSignatory, res.IsSignatory);
            Assert.Equal(role.IsFavorite, res.IsFavorite);
            Assert.Equal(role.IsDelegation, res.IsDelegation);
        }
    }

    [Fact]
    public void MapToRoles_WithNullSource_ShouldEmptyList()
    {
        var result = MapRoleDbToRoleBusiness.MapToRoles(null!);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
