// <copyright file="AccountTypeTranscriber.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Mappers;

public static class AccountTypeTranscriber
{
    private static readonly IReadOnlyDictionary<string, string> DbToExternal =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CLIENT"] = "REGULAR",
        };

    public static string? ToExternal(string? dbValue)
    {
        if (dbValue is null)
        {
            return null;
        }

        return DbToExternal.TryGetValue(dbValue, out var external) ? external : dbValue;
    }
}
