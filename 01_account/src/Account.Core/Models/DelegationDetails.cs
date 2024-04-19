// <copyright file="DelegationDetails.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Models
{
    public class DelegationDetails
    {
        public required int DelegateeId { get; set; }

        public DateTime? StartDate { get; set; }

        [JsonIgnore]
        public string? Status { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Note { get; set; }

        [JsonIgnore]
        public bool IsRoleToCreate { get; set; } = false;

        public bool IsAutomaticDelegation { get; set; }
    }
}
