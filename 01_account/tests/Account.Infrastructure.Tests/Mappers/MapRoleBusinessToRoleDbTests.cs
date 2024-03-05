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
        };

        // Act
        var result = role.MapRoleToRoleDb();

        // Assert
        Assert.Equivalent(expected, result);
    }
}
