// <copyright file="Pagination.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Extensions
{
    public static class Pagination
    {
        public static float GetTotalPages(int totalItems, int pageSize)
        {
            if (totalItems != 0)
            {
                var size = pageSize > totalItems ? totalItems : pageSize;
                return (float)totalItems / (float)size;
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
