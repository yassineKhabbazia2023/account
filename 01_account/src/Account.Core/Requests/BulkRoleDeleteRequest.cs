// <copyright file="BulkRoleDeleteRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Requests
{
    public class BulkRoleDeleteRequest
    {
        [Required]
        [MinLength(1)]
        public List<int> AccountIds { get; set; } = new();
    }
}
