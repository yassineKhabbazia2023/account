// <copyright file="AccountDetail.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Models
{
    public class AccountDetail
    {
        public int AccountId { get; set; }

        public Guid AccountGlobalUniqueId { get; set; }

        [NotEmptyOrWhiteSpace]
        public required string AccountNumber { get; set; }

        public string? IconName { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? AccountingOffice { get; set; }

        public int? EmployeeCount { get; set; }

        public string? CommercialName { get; set; }

        public Accounting? Accounting { get; set; }

        public required Legal Legal { get; set; }

        public Vat? Vat { get; set; }

        public IEnumerable<Address>? Address { get; set; }

        [MinLength(1, ErrorMessage = "Au moins un téléphone est requis.")]
        public required List<Phone> Phone { get; set; }

        public Hub? Hub { get; set; }

        public Deployment? Deployment { get; set; }

        public string? CreatedBy { get; set; } = string.Empty;

        public string? ModifiedBy { get; set; } = string.Empty;

        public decimal? Turnover { get; set; }

        public string? MissionType { get; set; }

        public int? OfficeId { get; set; }

        public Office? Office { get; set; }
    }
}
