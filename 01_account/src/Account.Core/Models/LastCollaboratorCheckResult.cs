// <copyright file="LastCollaboratorCheckResult.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class LastCollaboratorCheckResult
    {
        public bool IsLastCollaboratorOnAny { get; set; }

        public List<int> AccountIdsWhereLastCollaborator { get; set; } = new();
    }
}
