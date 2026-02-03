// <copyright file="Role.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Role
    {
        public int AccountId { get; set; }

        public int ContactId { get; set; }

        public bool? IsSignatory { get; set; }

        public bool? IsFavorite { get; set; }

        public bool? IsDelegation { get; set; }

        public bool? IsCustomerRelation { get; set; }

        public bool? ContactFlagPortailFactures { get; set; }

        public int ActionLevel { get; set; } = default;
    }
}
