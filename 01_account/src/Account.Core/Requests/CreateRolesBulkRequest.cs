// <copyright file="CreateRolesBulkRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Pulse.Account.Core.Requests
{
    public class CreateRolesBulkRequest
    {
        [Required]
        [MinLength(1)]
        public List<CreateRolesBulkItem> Contacts { get; set; } = new();
    }
}
