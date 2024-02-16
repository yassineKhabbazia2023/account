// <copyright file="PagesCalculator.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Extensions
{
    public static class PagesCalculator
    {
        public static float GetTotalPages(int count, int limit)
        {
            if(count != 0)
            {
                var size = limit > count ? count : (float)limit;
                return count / size;
            }

            return 0;
        }
    }
}
