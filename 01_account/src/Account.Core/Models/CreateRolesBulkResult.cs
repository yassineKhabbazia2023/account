// <copyright file="CreateRolesBulkResult.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class CreateRolesBulkResult
    {
        public List<CreateRolesBulkItemResult> Succeeded { get; set; } = new();

        public List<CreateRolesBulkItemResult> Failed { get; set; } = new();
    }
}
