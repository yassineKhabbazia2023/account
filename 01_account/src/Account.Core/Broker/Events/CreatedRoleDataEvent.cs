// <copyright file="CreatedRoleDataEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Broker.Events
{
    public class CreatedRoleDataEvent
    {
        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }

        [JsonPropertyName("accountId")]
        public int AccountId { get; set; }

        [JsonPropertyName("contactId")]
        public int ContactId { get; set; }

        [JsonPropertyName("isSignatory")]
        public bool? IsSignatory { get; set; }

        [JsonPropertyName("isFavorite")]
        public bool? IsFavorite { get; set; }

        [JsonPropertyName("isDelegation")]
        public bool? IsDelegation { get; set; }
    }
}
