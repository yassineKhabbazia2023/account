// <copyright file="ActionLevelHelperTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Extensions;
using InvalidOperationException = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

namespace Pulse.Account.Infrastructure.Tests.Extensions;

public class ActionLevelHelperTests
{
    [Fact]
    public void SetupActionLevel_WithWrongActionLevelAndNoLabelAndNoCustomerRelation_ShouldThrowInvalidOperationException()
    {
        var result = Assert.Throws<InvalidOperationException>(() => ActionLevelHelper.SetupActionLevel(-1, false, false));

        Assert.Equal("ACC036", result.Code);
        Assert.Equal("Le niveau d'action ne peut pas être inférieur à 0.", result.Message);
    }

    [Theory]
    [MemberData(nameof(ActionLevelData))]
    public void SetupActionLevel_Nominal(int expectedActionLevel, bool isCustomerRelation, bool hasRoleLabel, int expectedResult)
    {
        var result = ActionLevelHelper.SetupActionLevel(expectedActionLevel, isCustomerRelation, hasRoleLabel);

        Assert.Equal(expectedResult, result);
    }

    public static TheoryData<int, bool, bool, int> ActionLevelData => new TheoryData<int, bool, bool, int>
    {
        { 2, true, false, 4 },
        { 0, true, true, 4 },
        { 2, false, false, 2 },
        { 1, false, true, 3 },
        { -10, true, false, 4 },
        { -6, true, true, 4 },
        { -2, false, true, 3 }
    };
}
