// <copyright file="Pagination.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Extensions
{
    public static class Pagination
    {
        public static int GetTotalPages(float totalItems, float pageSize)
        {
            if (Math.Abs(totalItems - 0f) > float.Epsilon)
            {
                var size = pageSize > totalItems ? totalItems : pageSize;
                var totalPages = totalItems / size;
                return (int)Math.Ceiling(totalPages);
            }

            return 0;
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
