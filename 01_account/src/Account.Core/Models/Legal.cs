// <copyright file="Legal.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Models
{
    public class Legal
    {
        [NotEmptyOrWhiteSpace]
        public required string LegalName { get; set; }

        [NotEmptyOrWhiteSpace]
        public required string Siren { get; set; }

        public string? Siret { get; set; }

        public string? LegalForm { get; set; }

        public string? LegalFormCode { get; set; }

        public string? StaffSizeRange { get; set; }

        public ICollection<Naf>? Naf { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
