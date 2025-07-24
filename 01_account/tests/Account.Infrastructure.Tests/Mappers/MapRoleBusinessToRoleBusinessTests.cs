// <copyright file="MapRoleBusinessToRoleBusinessTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class MapRoleBusinessToRoleBusinessTests
{
    private readonly Fixture _fixture;

    public MapRoleBusinessToRoleBusinessTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void MapCreateRoleRequestToRole_ShouldMapCreateRoleRequestToRole()
    {
        var source = _fixture.Create<CreateRoleRequest>();

        var result = source.MapCreateRoleRequestToRole();

        Assert.NotNull(result);
        Assert.Equal(source.AccountId, result.AccountId);
        Assert.Equal(source.ContactId, result.ContactId);
        Assert.Equal(source.IsSignatory, result.IsSignatory);
        Assert.Equal(source.IsFavorite, result.IsFavorite);
        Assert.Equal(source.IsDelegation, result.IsDelegation);
        Assert.Equal(source.IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(source.ActionLevel, result.ActionLevel);
    }

    [Fact]
    public void MapCreateRoleRequestToRole_WithNullSource_ShouldReturnNull()
    {
        var result = MapRoleBusinessToRoleBusiness.MapCreateRoleRequestToRole(null!);
        Assert.Null(result);
    }
}
