// <copyright file="Account.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
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

        public ICollection<Address>? Address { get; set; }

        public Signatory? Signatory { get; set; }

        public ICollection<Deployment>? Deployment { get; set; }
    }
}
