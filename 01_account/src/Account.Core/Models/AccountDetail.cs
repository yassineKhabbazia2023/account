// <copyright file="AccountDetail.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Models
{
    public class AccountDetail
    {
        public int AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? IconName { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? AccountingOffice { get; set; }

        public int? EmployeeCount { get; set; }

        public string? CommercialName { get; set; }

        public Accounting? Accounting { get; set; }

        public Legal? Legal { get; set; }

        public Vat? Vat { get; set; }

        public IEnumerable<Address>? Address { get; set; }

        public IEnumerable<Phone>? Phone { get; set; }

        public Hub? Hub { get; set; }

        public IEnumerable<Deployment>? DeploymentPlanning { get; set; }
    }
}
