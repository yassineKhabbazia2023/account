// <copyright file="AccountUtils.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Utils
{
    public static class AccountUtils
    {
        public static float CalculTotalPage(int count, int limit)
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
