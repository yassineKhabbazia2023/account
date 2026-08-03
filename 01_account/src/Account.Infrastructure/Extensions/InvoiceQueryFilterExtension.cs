// <copyright file="InvoiceQueryFilterExtension.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Extensions;

public static class InvoiceQueryFilterExtension
{
    public static IQueryable<InvoiceEntity> ApplyFilters(this IQueryable<InvoiceEntity> query, string? search)
    {
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.InvoiceNumber.Contains(search) || i.Category.Contains(search) || i.Type.Contains(search));
        }

        return query;
    }
}
