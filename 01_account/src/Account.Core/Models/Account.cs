// <copyright file="Account.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Kpmg.Account.Core.Models
{
    public class Account
    {
        public Guid? AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? LegalName { get; set; }

        public bool? IsFavorite { get; set; }

        public IEnumerable<Address>? Address { get; set; }

        public Owner? Owner { get; set; }

        public IEnumerable<Deployment>? Deployment { get; set; }
    }
}
