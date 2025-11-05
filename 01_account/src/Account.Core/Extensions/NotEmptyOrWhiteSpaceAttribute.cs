// <copyright file="NotEmptyOrWhiteSpaceAttribute.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Extensions;

using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class NotEmptyOrWhiteSpaceAttribute : ValidationAttribute
{
    public NotEmptyOrWhiteSpaceAttribute()
    {
        ErrorMessage = Exceptions.Errors.InvalidAccountFieldsMessage;
    }

    public override bool IsValid(object? value)
    {
        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        return false;
    }
}