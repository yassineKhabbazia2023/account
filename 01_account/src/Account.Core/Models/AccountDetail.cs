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

        public string? LegalName { get; set; }

        public string? LegalFormCode { get; set; }

        public string? SiretNumber { get; set; }

        public string? NafCode { get; set; }

        public int StaffSizeRange { get; set; }

        public string? HubName { get; set; }

        public Accounting? Accounting { get; set; }

        public Vat? Vat { get; set; }
    }
}
