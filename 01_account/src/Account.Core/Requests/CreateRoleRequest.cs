// <copyright file="CreateRoleRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Requests
{
    public class CreateRoleRequest
    {
        public int AccountId { get; set; }

        [JsonIgnore]
        public Guid? AccountGlobalUniqueId { get; set; }

        public int ContactId { get; set; }

        public string? Email { get; set; }

        [JsonIgnore]
        public Guid? ContactGlobalUniqueId { get; set; }

        public bool? IsSignatory { get; set; }

        public bool? IsFavorite { get; set; }

        public bool? IsDelegation { get; set; }

        public int? DelegatorId { get; set; }
    }
}
