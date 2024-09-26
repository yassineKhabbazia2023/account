// <copyright file="GlobalConstantsTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Core.Constants;

namespace Account.Api.Tests.Configurations.Models;
public class GlobalConstantsTests
{
    [Fact]
    public void Constants_ShouldAlwaysBeTheSame()
    {
        GlobalConstants.RETRYCOUNT.Should().NotBe(0);
        GlobalConstants.RETRYTIMESPAN.Should().NotBe(0);
        GlobalConstants.RETRYCOUNT.Should().BeGreaterThanOrEqualTo(1);
        GlobalConstants.RETRYTIMESPAN.Should().BeGreaterThanOrEqualTo(3000);
    }
}
