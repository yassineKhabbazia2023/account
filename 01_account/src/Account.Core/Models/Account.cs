// <copyright file="Account.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Pulse.Account.Core.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? LegalName { get; set; }

        public bool? IsFavorite { get; set; }

        public IEnumerable<Address>? Address { get; set; }

        public Contact? Signatory { get; set; }

        public IEnumerable<Deployment>? Deployment { get; set; }
    }
}
