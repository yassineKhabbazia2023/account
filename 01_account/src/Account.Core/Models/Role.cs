// <copyright file="Role.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        public int AccountId { get; set; }

        public int ContactId { get; set; }

        public bool? IsSignatory { get; set; }

        public bool? IsFavorite { get; set; }
    }
}
