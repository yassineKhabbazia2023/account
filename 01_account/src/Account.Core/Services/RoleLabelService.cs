// <copyright file="RoleLabelService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Services
{
    public class RoleLabelService : IRoleLabelService
    {
        private readonly IRoleLabelRepository _roleLabelRepository;

        public RoleLabelService(IRoleLabelRepository roleLabelRepository)
        {
            _roleLabelRepository = roleLabelRepository;
        }

        public async Task AddRoleLabelAsync(RoleLabel roleLabel)
        {
            await _roleLabelRepository.AddRoleLabelAsync(roleLabel);
        }

        public async Task DeleteRoleLabelAsync(int accountId, int contactId, int labelId)
        {
            await _roleLabelRepository.DeleteRoleLabelAsync(accountId, contactId, labelId);
        }
    }
}
