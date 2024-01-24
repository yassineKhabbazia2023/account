// <copyright file="Account.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Kpmg.Account.Core.Models
{
    public class Account
    {
        public string? AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? LegalName { get; set; }

        public bool IsFavorite { get; set; }

        public Address? Address { get; set; }

        public Owner? Owner { get; set; }

        public Deployment? Deployment { get; set; }
    }
}
