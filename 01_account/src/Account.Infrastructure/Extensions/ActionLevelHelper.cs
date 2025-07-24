// <copyright file="ActionLevelHelper.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using InvalidOperationException = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

namespace Pulse.Account.Infrastructure.Extensions;

public static class ActionLevelHelper
{
    public static int SetupActionLevel(int expectedActionLevel, bool isCustomerRelation, bool hasRoleLabel)
    {
        if (isCustomerRelation)
        {
            return (int)ActionLevelType.DirectClientRelation;
        }

        if (hasRoleLabel)
        {
            return (int)ActionLevelType.Contributor;
        }

        if (expectedActionLevel < 0)
        {
            throw new InvalidOperationException(Errors.InvalidActionLevelCode, Errors.InvalidActionLevelMessage);
        }

        return expectedActionLevel;
    }
}
