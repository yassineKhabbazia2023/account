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

        public required List<Phone> Phone { get; set; }

        public Hub? Hub { get; set; }

        public Deployment? Deployment { get; set; }

        public string? CreatedBy { get; set; } = string.Empty;

        public string? ModifiedBy { get; set; } = string.Empty;

        public string? AccountType { get; set; }

        public decimal? Turnover { get; set; }

        public string? MissionType { get; set; }

        public int? OfficeId { get; set; }

        public Office? Office { get; set; }

        /// <summary>
        /// Gets or sets the Akuiteo routing code. The current maximum length of 255 is provisional pending the
        /// definitive Akuiteo contract.
        /// </summary>
        public string? AccountRoutingCode { get; set; }

        /// <summary>
        /// Gets or sets the Akuiteo routing label. The current maximum length of 255 is provisional pending the
        /// definitive Akuiteo contract.
        /// </summary>
        public string? AccountRoutingLabel { get; set; }

        /// <summary>
        /// Gets or sets the Akuiteo legal-form label. The current maximum length of 255 is provisional pending the
        /// definitive Akuiteo contract.
        /// </summary>
        public string? AccountLegalFormLabel { get; set; }

        /// <summary>
        /// Gets or sets the raw Akuiteo electronic address identifier, as stored in database: a list of sites
        /// flattened into a single string, one "-"-separated line (site name, site code, SIRET, addressing id)
        /// per site, lines separated by "/".
        /// </summary>
        public string? AccountElectronicAddressId { get; set; }

        /// <summary>
        /// Gets the electronic addresses per site, transcoded from <see cref="AccountElectronicAddressId"/> into a
        /// JSON array exploitable by the Front-End.
        /// </summary>
        public IReadOnlyList<ElectronicAddress> AccountElectronicAddresses => AccountElectronicAddressId.ToElectronicAddresses();
    }
}
