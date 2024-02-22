// <copyright file="PagesCalculator.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Extensions
{
    public static class PagesCalculator
    {
        public static float GetTotalPages(int totalRows, int pageSize)
        {
            if(totalRows != 0)
            {
                var size = pageSize > totalRows ? totalRows : (float)pageSize;
                return totalRows / size;
            }

            return 0;
        }
    }
}
