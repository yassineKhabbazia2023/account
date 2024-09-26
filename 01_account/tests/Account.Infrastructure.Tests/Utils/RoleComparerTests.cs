// <copyright file="RoleComparerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Tests.Utils;

using FluentAssertions;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Utils;

public class RoleComparerTests
{
    private readonly RoleComparer _comparer = new RoleComparer();

    [Fact]
    public void Equals_BothNull_ShouldReturnTrue()
    {
        _comparer.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void Equals_OneNull_ShouldReturnFalse()
    {
        var role = new RoleEntity { AccountId = 1, ContactId = 1 };
        _comparer.Equals(role, null).Should().BeFalse();
        _comparer.Equals(null, role).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameValues_ShouldReturnTrue()
    {
        var role1 = new RoleEntity { AccountId = 1, ContactId = 1 };
        var role2 = new RoleEntity { AccountId = 1, ContactId = 1 };
        _comparer.Equals(role1, role2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ShouldReturnFalse()
    {
        var role1 = new RoleEntity { AccountId = 1, ContactId = 1 };
        var role2 = new RoleEntity { AccountId = 1, ContactId = 2 };
        var role3 = new RoleEntity { AccountId = 2, ContactId = 1 };

        _comparer.Equals(role1, role2).Should().BeFalse();
        _comparer.Equals(role1, role3).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameValues_ShouldReturnSameHashCode()
    {
        var role1 = new RoleEntity { AccountId = 1, ContactId = 1 };
        var role2 = new RoleEntity { AccountId = 1, ContactId = 1 };

        _comparer.GetHashCode(role1).Should().Be(_comparer.GetHashCode(role2));
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHashCodes()
    {
        var role1 = new RoleEntity { AccountId = 1, ContactId = 1 };
        var role2 = new RoleEntity { AccountId = 1, ContactId = 2 };
        var role3 = new RoleEntity { AccountId = 2, ContactId = 1 };

        _comparer.GetHashCode(role1).Should().NotBe(_comparer.GetHashCode(role2));
        _comparer.GetHashCode(role1).Should().NotBe(_comparer.GetHashCode(role3));
    }

    [Fact]
    public void UseInHashSet_ShouldWorkCorrectly()
    {
        var role1 = new RoleEntity { AccountId = 1, ContactId = 1 };
        var role2 = new RoleEntity { AccountId = 1, ContactId = 1 };
        var role3 = new RoleEntity { AccountId = 2, ContactId = 2 };

        var hashSet = new HashSet<RoleEntity>(_comparer)
        {
            role1,
            role2,
            role3
        };

        hashSet.Should().HaveCount(2);
        hashSet.Should().Contain(role1);
        hashSet.Should().Contain(role3);
    }
}
