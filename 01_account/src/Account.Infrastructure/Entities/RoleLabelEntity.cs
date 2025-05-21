// <copyright file="RoleLabelEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Entities
{
    public class RoleLabelEntity
    {
        public required int AccountId { get; set; }

        public required int ContactId { get; set; }

        public required int LabelId { get; set; }

        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public required int CreatedBy { get; set; }

        public virtual AccountEntity? AccountEntity { get; set; }

        public virtual ContactEntity? ContactEntity { get; set; }

        public virtual LabelEntity? LabelEntity { get; set; }

        public virtual ContactEntity? CollaboratorEntity { get; set; }
    }
}
