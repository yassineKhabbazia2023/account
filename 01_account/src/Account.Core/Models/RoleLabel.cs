// <copyright file="RoleLabel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class RoleLabel
    {
        public required int AccountId { get; set; }

        public required int ContactId { get; set; }

        public required int LabelId { get; set; }

        public DateTime CreatedDate { get; set; }

        public required int CreatedBy { get; set; }
    }
}
