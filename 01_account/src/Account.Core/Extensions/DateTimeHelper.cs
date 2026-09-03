// <copyright file="DateTimeHelper.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Extensions;

public static class DateTimeHelper
{
    private const string ParisTimeZone = "Europe/Paris";

    public static DateTime ConvertToLocalizedTime(DateTime utcDate)
    {
        var utc = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(ParisTimeZone);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
    }
}
