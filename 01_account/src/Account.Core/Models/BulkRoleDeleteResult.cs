// <copyright file="BulkRoleDeleteResult.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class BulkRoleDeleteResult
    {
        public List<BulkRoleDeleteItemResult> Succeeded { get; set; } = new();

        public List<BulkRoleDeleteItemResult> Failed { get; set; } = new();
    }
}
