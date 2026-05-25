// <copyright file="AccountTypeTranscriberTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class AccountTypeTranscriberTests
{
    [Theory]
    [InlineData("CLIENT", "REGULAR")]
    [InlineData("client", "REGULAR")]
    [InlineData("Client", "REGULAR")]
    public void ToExternal_WithKnownDbValue_ReturnsExternalLabel(string dbValue, string expected)
    {
        AccountTypeTranscriber.ToExternal(dbValue).Should().Be(expected);
    }

    [Theory]
    [InlineData("PROSPECT")]
    [InlineData("UNKNOWN")]
    [InlineData("")]
    public void ToExternal_WithUnmappedDbValue_ReturnsValueUnchanged(string dbValue)
    {
        AccountTypeTranscriber.ToExternal(dbValue).Should().Be(dbValue);
    }

    [Fact]
    public void ToExternal_WithNull_ReturnsNull()
    {
        AccountTypeTranscriber.ToExternal(null).Should().BeNull();
    }
}
