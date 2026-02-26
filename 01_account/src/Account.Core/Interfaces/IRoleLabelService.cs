// <copyright file="IRoleLabelService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IRoleLabelService
    {
        Task DeleteRoleLabelAsync(int accountId, int contactId, int labelId);

        Task AddRoleLabelAsync(RoleLabel roleLabel);

        Task<bool> HasRoleLabel(int contactId, int accountId, int labelId);

        Task RevokeExclusiveLabelAsync(int accountId, int labelId, string labelCode);
    }
}
