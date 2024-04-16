// <copyright file="SearchAccountCriteria.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Models.Utils
{
    public class SearchAccountCriteria
    {
        public string? search { get; set; }

        public int pageNumber { get; set; }

        public int pageSize { get; set; }

        public int? deploymentStatus { get; set; }

        [Required]
        public int contactId { get; set; }
    }
}
