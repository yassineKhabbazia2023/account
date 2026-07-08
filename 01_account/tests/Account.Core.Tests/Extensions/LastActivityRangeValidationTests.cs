// <copyright file="LastActivityRangeValidationTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Extensions;

public class LastActivityRangeValidationTests
{
    private static readonly DateTime UtcDate = new(2026, 7, 8, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_WhenBoundsAreUtcAndOrdered_DoesNotThrow()
    {
        LastActivityRangeValidation.Validate(UtcDate, UtcDate.AddDays(1));
    }

    [Fact]
    public void Validate_WhenBoundsAreNull_DoesNotThrow()
    {
        LastActivityRangeValidation.Validate(null, null);
    }

    [Fact]
    public void Validate_WhenFromIsAfterTo_ThrowsBadRequestException()
    {
        var exception = Assert.Throws<BadRequestException>(
            () => LastActivityRangeValidation.Validate(UtcDate.AddDays(1), UtcDate));

        Assert.Equal(Errors.BadRequestLastActivityRangeCode, exception.Code);
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Local)]
    public void Validate_WhenFromIsNotUtc_ThrowsBadRequestException(DateTimeKind kind)
    {
        var from = DateTime.SpecifyKind(UtcDate, kind);

        var exception = Assert.Throws<BadRequestException>(
            () => LastActivityRangeValidation.Validate(from, null));

        Assert.Equal(Errors.BadRequestLastActivityUtcCode, exception.Code);
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Local)]
    public void Validate_WhenToIsNotUtc_ThrowsBadRequestException(DateTimeKind kind)
    {
        var to = DateTime.SpecifyKind(UtcDate, kind);

        var exception = Assert.Throws<BadRequestException>(
            () => LastActivityRangeValidation.Validate(null, to));

        Assert.Equal(Errors.BadRequestLastActivityUtcCode, exception.Code);
    }
}
