// <copyright file="Legal.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Account.Core.Models
{
    public class Legal
    {
        public string? LegalName { get; set; }

        public string? Siren { get; set; }

        public string? Siret { get; set; }

        public string? LegalForm { get; set; }

        public string? LegalFormCode { get; set; }

        public string? StaffSizeRange { get; set; }

        public ICollection<Naf>? Naf { get; set; }
    }
}
