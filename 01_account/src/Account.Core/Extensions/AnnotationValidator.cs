// <copyright file="AnnotationValidator.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Pulse.Account.Core.Extensions;

public static class AnnotationValidator
{
    public static bool TryValidateObjectRecursive<T>(T obj, List<ValidationResult> results)
    {
        if (EqualityComparer<T>.Default.Equals(obj, default(T)))
        {
            return false;
        }

        bool result = TryValidate(obj, results);

        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            object? value = property.GetValue(obj, null);
            if (value == null)
            {
                continue;
            }

            if (value is IEnumerable<object> asEnumerable)
            {
                result = ValidateEnumerable(property, asEnumerable, results) && result;
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                result = TryValidateObjectRecursive(value, results) && result;
            }
        }

        return result;
    }

    private static bool ValidateEnumerable(PropertyInfo property, IEnumerable<object> asEnumerable, List<ValidationResult> results)
    {
        bool result = true;
        var enumerable = asEnumerable.ToList();

        foreach (var enumObj in enumerable)
        {
            result = TryValidateObjectRecursive(enumObj, results) && result;
        }

        return result;
    }

    private static bool TryValidate(object obj, List<ValidationResult> results)
    {
        var context = new ValidationContext(obj, null, null);
        return Validator.TryValidateObject(obj, context, results, true);
    }
}