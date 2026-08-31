// <copyright file="StringExtensions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Globalization;

namespace Pulse.Account.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Capitalizes each word in the string (first letter uppercase, rest lowercase).
    /// </summary>
    /// <param name="value">The string to capitalize.</param>
    /// <returns>The capitalized string.</returns>
    public static string ToCapitalize(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return CultureInfo.GetCultureInfo("fr-FR").TextInfo.ToTitleCase(value.ToLower(CultureInfo.GetCultureInfo("fr-FR")));
    }
}
