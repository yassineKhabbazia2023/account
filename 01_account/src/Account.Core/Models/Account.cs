// <copyright file="Account.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public Guid? AccountGlobalUniqueId { get; set; }

        public string? AccountNumber { get; set; }

        public string? LegalName { get; set; }

        public bool? IsFavorite { get; set; }

        public Address? Address { get; set; }

        public Contact? Signatory { get; set; }

        public Deployment? Deployment { get; set; }

        public Hub? Hub { get; set; }
    }
}
