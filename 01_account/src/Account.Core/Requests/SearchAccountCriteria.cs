// <copyright file="SearchAccountCriteria.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Requests
{
    public class SearchAccountCriteria
    {
        public string? Search { get; set; }

        public int? DeploymentStatus { get; set; }

        [Required]
        public int ContactId { get; set; }

        public bool? IsFavoriteFilter { get; set; }

        public bool? IsCustomerRelationFilter { get; set; }

        public Sorting? Sorting { get; set; }

    }
}
