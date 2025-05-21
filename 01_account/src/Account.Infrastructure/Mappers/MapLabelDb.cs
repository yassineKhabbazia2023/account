// <copyright file="MapLabelDb.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapLabelDb
    {
        public static LabelEntity? Map(this Label label)
        {
            return label == null ? null : new LabelEntity()
            {
                Code = label.Code,
                CollaboratorLabel = label.CollaboratorLabel,
                CustomerLabel = label.CustomerLabel,
                Description = label?.Description,
                LabelId = label.LabelId,
                Business = label.Business,
                IsVisible = label.IsVisible
            };
        }

        public static Label? Map(this LabelEntity label)
        {
            return label == null ? null : new Label()
            {
                Code = label.Code,
                CollaboratorLabel = label.CollaboratorLabel,
                CustomerLabel = label.CustomerLabel,
                Description = label.Description,
                LabelId = label.LabelId,
                Business = label.Business,
                IsVisible = label.IsVisible
            };
        }

        public static RoleLabel? Map(this RoleLabelEntity roleLabel)
        {
            return roleLabel == null ? null : new RoleLabel
            {
                AccountId = roleLabel.AccountId,
                ContactId = roleLabel.ContactId,
                CreatedBy = roleLabel.CreatedBy,
                CreatedDate = roleLabel.CreatedDate,
                LabelId = roleLabel.LabelId,
            };
        }

        public static RoleLabelEntity? Map(this RoleLabel roleLabel)
        {
            return roleLabel == null ? null : new RoleLabelEntity
            {
                AccountId = roleLabel.AccountId,
                ContactId = roleLabel.ContactId,
                CreatedBy = roleLabel.CreatedBy,
                CreatedDate = roleLabel.CreatedDate,
                LabelId = roleLabel.LabelId
            };
        }
    }
}
