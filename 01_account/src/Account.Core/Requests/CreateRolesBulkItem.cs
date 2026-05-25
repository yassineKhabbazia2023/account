// <copyright file="CreateRolesBulkItem.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Requests
{
    public class CreateRolesBulkItem
    {
        [Required]
        public int ContactId { get; set; }

        public bool? IsSignatory { get; set; }

        public bool? IsFavorite { get; set; }

        public bool? IsDelegation { get; set; }

        public bool? IncludePennylaneAccess { get; set; }

        [RegularExpression(@"^(AM|CLP)$", ErrorMessage = "RoleCode doit valoir AM ou CLP.")]
        public string? RoleCode { get; set; }
    }
}
