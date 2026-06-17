// <copyright file="MissionTypeValidation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Extensions
{
    public static class MissionTypeValidation
    {
        public static string? GetValidMissionType(string? missionType)
        {
            return string.IsNullOrWhiteSpace(missionType) || System.Enum.IsDefined(typeof(MissionType), missionType)
                ? missionType
                : throw new BadRequestException(Errors.BadRequestMissionTypeCode, string.Format(Errors.BadRequestMissionTypeMessage, missionType));
        }
    }
}
