// <copyright file="StringExtensionsTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToCapitalize_WhenNullOrWhiteSpace_ShouldReturnEmpty(string? value)
    {
        var result = value.ToCapitalize();

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("john", "John")]
    [InlineData("JOHN", "John")]
    [InlineData("jOhN", "John")]
    [InlineData("john doe", "John Doe")]
    [InlineData("JEAN-PIERRE", "Jean-Pierre")]
    public void ToCapitalize_WhenValidValue_ShouldCapitalizeEachWord(string value, string expected)
    {
        var result = value.ToCapitalize();

        result.Should().Be(expected);
    }
}
