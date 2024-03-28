// <copyright file="Pagination.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Extensions
{
    public static class Pagination
    {
        public static int GetTotalPages(int totalItems, int pageSize)
        {
            if (totalItems <= 0 || pageSize <= 0)
            {
                return 0;
            }

            int totalPages = totalItems / pageSize;
            if (totalItems % pageSize != 0)
            {
                totalPages++;
            }

            return totalPages;
        }

        public static int GetValidPageNumber(int pageNumber)
        {
            return Math.Max(1, pageNumber);
        }

        public static int GetValidPageSize(int pageSize)
        {
            return pageSize <= 0 ? int.MaxValue : pageSize;
        }
    }
}
