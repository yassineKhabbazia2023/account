// <copyright file="SearchAccountCriteria.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Models.Utils
{
    public class SearchAccountCriteria
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string? search { get; set; }

        public int pageNumber { get; set; }

        public int pageSize { get; set; }

        public int? deploymentStatus { get; set; }

        [Required]
        public int contactId { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
