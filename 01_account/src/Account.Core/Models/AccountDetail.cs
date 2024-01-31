// <copyright file="AccountDetail.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kpmg.Account.Core.Models
{
    public class AccountDetail
    {
        public string? AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? IconName { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? AccountNumberSource { get; set; }

        public string? AccountingOffice { get; set; }

        public int EmployeeCount { get; set; }

        public string? CommercialName { get; set; }

        public Accounting? Accounting { get; set; }

        public Legal? Legal { get; set; }

        public Vat? Vat { get; set; }

        public ICollection<Address>? Address { get; set; }

        public ICollection<Phone>? Phone { get; set; }

        public Hub? Hub { get; set; }

        public ICollection<Deployment>? DeploymentPlanning { get; set; }
    }
}
