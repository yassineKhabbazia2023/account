// <copyright file="RoleComparer.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Utils
{
    public class RoleComparer : IEqualityComparer<RoleEntity>
    {
        public bool Equals(RoleEntity? x, RoleEntity? y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null || y == null)
            {
                return false;
            }
            else
            {
                return x.AccountId == y.AccountId && x.ContactId == y.ContactId;
            }
        }

        public int GetHashCode(RoleEntity obj)
        {
            return obj.AccountId.GetHashCode() ^ obj.ContactId.GetHashCode();
        }
    }
}
