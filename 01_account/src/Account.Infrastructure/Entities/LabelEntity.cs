// <copyright file="LabelEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Entities
{
    public class LabelEntity
    {
        public int LabelId { get; set; }

        public required string Code { get; set; }

        public required string CustomerLabel { get; set; }

        public required string CollaboratorLabel { get; set; }

        public required string Business { get; set; }

        public required bool IsVisible { get; set; } = true;

        public string? Description { get; set; } = null;

        public virtual ICollection<RoleLabelEntity>? RoleLabelEntities { get; set; }
    }
}
