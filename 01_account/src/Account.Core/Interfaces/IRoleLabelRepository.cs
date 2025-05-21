// <copyright file="IRoleLabelRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IRoleLabelRepository
    {
        Task DeleteRoleLabelAsync(int accountId, int contactId, int labelId);

        Task AddRoleLabelAsync(RoleLabel roleLabel);
    }
}
