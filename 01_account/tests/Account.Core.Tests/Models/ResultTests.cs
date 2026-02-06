// <copyright file="ResultTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Core.Tests.Models;

public class ResultTests
{
    [Fact]
    public void Success_Should_SetStatusValueAndIsSuccess()
    {
        var result = Result<int>.Success(42);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void NotFound_Should_SetStatusAndDefaultValue()
    {
        var result = Result<string>.NotFound();

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
    }
}
