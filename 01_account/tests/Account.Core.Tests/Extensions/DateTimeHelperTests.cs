// <copyright file="DateTimeHelperTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Tests.Extensions;

public class DateTimeHelperTests
{
    [Fact]
    public void ConvertToLocalizedTime_WhenWinterDate_ShouldApplyGmtPlusOne()
    {
        var utcDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var result = DateTimeHelper.ConvertToLocalizedTime(utcDate);

        result.Should().Be(new DateTime(2024, 1, 15, 11, 0, 0));
    }

    [Fact]
    public void ConvertToLocalizedTime_WhenSummerDate_ShouldApplyGmtPlusTwo()
    {
        var utcDate = new DateTime(2024, 7, 15, 10, 0, 0, DateTimeKind.Utc);

        var result = DateTimeHelper.ConvertToLocalizedTime(utcDate);

        result.Should().Be(new DateTime(2024, 7, 15, 12, 0, 0));
    }

    [Fact]
    public void ConvertToLocalizedTime_WhenKindIsUnspecified_ShouldTreatValueAsUtc()
    {
        var unspecifiedDate = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);

        var result = DateTimeHelper.ConvertToLocalizedTime(unspecifiedDate);

        result.Should().Be(new DateTime(2024, 1, 15, 11, 0, 0));
    }
}
