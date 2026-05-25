// <copyright file="CreateRolesBulkItemResult.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class CreateRolesBulkItemResult
    {
        public int ContactId { get; set; }

        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
