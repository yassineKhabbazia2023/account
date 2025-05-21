// <copyright file="RoleLabelDeleteRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class RoleLabelDeleteRequest
    {
        public required int AccountId { get; set; }

        public required int ContactId { get; set; }

        public required int LabelId { get; set; }
    }
}
