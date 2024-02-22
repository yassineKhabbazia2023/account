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
    public void MapTContactToSignatory_CaseSuccess()
    {
        // Arrange
        CreateRole? role = _fixture.Create<CreateRole?>();
        var expected = new TRole()
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
