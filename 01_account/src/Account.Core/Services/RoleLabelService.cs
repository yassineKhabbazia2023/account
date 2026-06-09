// <copyright file="RoleLabelService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services
{
    public class RoleLabelService : IRoleLabelService
    {
        private static readonly HashSet<string> ExclusiveLabelCodes = new(StringComparer.OrdinalIgnoreCase) { "CLP", "AM" };

        private readonly IRoleLabelRepository _roleLabelRepository;
        private readonly ILabelService _labelService;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RoleLabelService> _logger;

        public RoleLabelService(
            IRoleLabelRepository roleLabelRepository,
            ILabelService labelService,
            IRoleRepository roleRepository,
            ILogger<RoleLabelService> logger)
        {
            _roleLabelRepository = roleLabelRepository;
            _labelService = labelService;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task AddRoleLabelAsync(RoleLabel roleLabel)
        {
            await _roleLabelRepository.AddRoleLabelAsync(roleLabel);
        }

        public async Task DeleteRoleLabelAsync(int accountId, int contactId, int labelId)
        {
            var labelCode = await _roleLabelRepository.GetLabelCodeAsync(labelId);

            if (labelCode is not null && ExclusiveLabelCodes.Contains(labelCode) && await _roleRepository.IsProspectAccountAsync(accountId))
            {
                throw new BadRequestException(Errors.CannotDeleteExclusiveLabelProspectCode, Errors.CannotDeleteExclusiveLabelProspectMessage);
            }

            await _roleLabelRepository.DeleteRoleLabelAsync(accountId, contactId, labelId);
        }

        public async Task<bool> HasRoleLabel(int contactId, int accountId, int labelId)
        {
            return await _roleLabelRepository.HasRoleLabel(contactId, accountId, labelId);
        }

        public async Task RevokeExclusiveLabelAsync(int accountId, int labelId, string labelCode)
        {
            if (!ExclusiveLabelCodes.Contains(labelCode))
            {
                return;
            }

            await _roleLabelRepository.RemoveLabelAssignmentFromAccountAsync(accountId, labelId);
        }

        public async Task<bool> HasExclusiveLabelAsync(int accountId, int contactId)
        {
            return await _roleLabelRepository.HasExclusiveLabelAsync(accountId, contactId);
        }

        public async Task AssignRoleLabelFromCodeAsync(string? code, int accountId, int contactId)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            var labels = (await _labelService.GetLabelsAsync(new Pagination())).Items ?? Enumerable.Empty<Label>();
            var label = labels.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (label == null)
            {
                _logger.LogWarning("Label avec le code {Code} introuvable en base pour le rôle AccountId: {AccountId} - ContactId: {ContactId}", code, accountId, contactId);
                return;
            }

            if (await HasRoleLabel(contactId, accountId, label.LabelId))
            {
                _logger.LogWarning("Le label {Code} est déjà affecté au rôle AccountId: {AccountId} - ContactId: {ContactId}", code, accountId, contactId);
                return;
            }

            await RevokeExclusiveLabelAsync(accountId, label.LabelId, label.Code);

            await AddRoleLabelAsync(new RoleLabel
            {
                AccountId = accountId,
                ContactId = contactId,
                LabelId = label.LabelId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = contactId,
            });

            _logger.LogInformation("Le libellé {Code} a été ajouté sur le rôle AccountId {AccountId}/ContactId {ContactId}", code, accountId, contactId);
        }
    }
}
