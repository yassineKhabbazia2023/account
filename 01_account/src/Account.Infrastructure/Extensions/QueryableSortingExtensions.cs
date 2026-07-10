// <copyright file="QueryableSortingExtensions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq.Expressions;

namespace Pulse.Account.Infrastructure.Extensions;

/// <summary>
/// Tri directionnel composable sur IQueryable.
/// </summary>
public static class QueryableSortingExtensions
{
    public static IOrderedQueryable<T> OrderByDirection<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool descending)
    {
        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
