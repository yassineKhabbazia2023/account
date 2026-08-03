// <copyright file="SortMap.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq.Expressions;

namespace Pulse.Account.Infrastructure.Extensions;

public sealed class SortMap<TEntity>
{
    private readonly Dictionary<string, LambdaExpression> _selectors = new(StringComparer.OrdinalIgnoreCase);
    private string _defaultField = string.Empty;

    public SortMap<TEntity> Add<TKey>(string field, Expression<Func<TEntity, TKey>> selector, bool isDefault = false)
    {
        _selectors[field] = selector;

        if (isDefault || _defaultField.Length == 0)
        {
            _defaultField = field;
        }

        return this;
    }

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, string? field, string? order)
    {
        if (field is null || !_selectors.TryGetValue(field, out var selector))
        {
            selector = _selectors[_defaultField];
        }

        var descending = !string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase);
        var method = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var call = Expression.Call(
            typeof(Queryable),
            method,
            [typeof(TEntity), selector.ReturnType],
            query.Expression,
            Expression.Quote(selector));

        return query.Provider.CreateQuery<TEntity>(call);
    }
}
