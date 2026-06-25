// <copyright file="MissionTypeValidation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Extensions;

public static class MissionTypeValidation
{
    public static List<string>? GetValidMissionTypes(List<string>? missionTypes)
    {
        if (missionTypes == null)
        {
            return null;
        }

        var cleaned = missionTypes.Where(missionType => !string.IsNullOrWhiteSpace(missionType)).ToList();

        var firstInvalid = cleaned.FirstOrDefault(missionType => !System.Enum.IsDefined(typeof(MissionType), missionType));

        if (firstInvalid != null)
        {
            throw new BadRequestException(Errors.BadRequestMissionTypeCode, string.Format(Errors.BadRequestMissionTypeMessage, firstInvalid));
        }

        return cleaned;
    }
}
