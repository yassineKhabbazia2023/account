// <copyright file="MapAccountSearchResultDb.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Pulse.Account.Infrastructure.Mappers;

public static class MapAccountSearchResultDb
{
    public static Paging<AccountSearchResult> MapToPagingAccountSearchResult(
        this IEnumerable<AccountSearchResult> source,
        int pageNumber,
        int totalRows,
        int totalPageCalcul)
    {
        return new Paging<AccountSearchResult>
        {
            Items = source,
            CurrentPage = pageNumber,
            TotalItems = totalRows,
            TotalPage = totalPageCalcul
        };
    }
}
