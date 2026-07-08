// <copyright file="LastActivityRangeValidation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Account.Core.Exceptions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Extensions
{
    public static class LastActivityRangeValidation
    {
        public static void Validate(DateTime? from, DateTime? to)
        {
            if (IsNotUtc(from) || IsNotUtc(to))
            {
                throw new BadRequestException(Errors.BadRequestLastActivityUtcCode, Errors.BadRequestLastActivityUtcMessage);
            }

            if (from > to)
            {
                throw new BadRequestException(Errors.BadRequestLastActivityRangeCode, Errors.BadRequestLastActivityRangeMessage);
            }
        }

        private static bool IsNotUtc(DateTime? bound)
        {
            return bound != null && bound.Value.Kind != DateTimeKind.Utc;
        }
    }
}
