// <copyright file="RoleLabelRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class RoleLabelRepository : IRoleLabelRepository
    {
        private readonly AccountContext _accountContext;

        public RoleLabelRepository(AccountContext accountContext)
        {
            _accountContext = accountContext;
        }

        public async Task AddRoleLabelAsync(RoleLabel roleLabel)
        {
            if (roleLabel is null)
            {
                throw new ArgumentNullException(Errors.NullArgumentCode, string.Format(Errors.NullArgumentMessage, nameof(roleLabel)));
            }

            bool roleLabelexists = await _accountContext.RoleLabelEntity
                .AsNoTracking().AnyAsync(role => role.AccountId == roleLabel.AccountId && role.ContactId == roleLabel.ContactId && role.LabelId == roleLabel.LabelId);

            if (roleLabelexists)
            {
                throw new ConflictException(Errors.RoleLabelAlreadyExistsCode, Errors.RoleLabelAlreadyExistsMessage);
            }

            bool roleExists = await _accountContext.RoleEntity.AsNoTracking().AnyAsync(role => role.AccountId == roleLabel.AccountId && role.ContactId == roleLabel.ContactId);

            if (!roleExists)
            {
                throw new NotFoundException(Errors.RoleNotFoundCode, string.Format(Errors.RoleNotFoundMessage, roleLabel.AccountId, roleLabel.ContactId));
            }

            var roleLabelEntity = roleLabel.Map();

            await _accountContext.RoleLabelEntity.AddAsync(roleLabelEntity);
            await _accountContext.SaveChangesAsync();
        }

        public async Task DeleteRoleLabelAsync(int accountId, int contactId, int labelId)
        {
            var roleLabel = await _accountContext.RoleLabelEntity
               .FirstOrDefaultAsync(role => role.AccountId == accountId && role.ContactId == contactId && role.LabelId == labelId);

            if (roleLabel is not null)
            {
                _accountContext.RoleLabelEntity.Remove(roleLabel);
                await _accountContext.SaveChangesAsync();
            }
        }
    }
}
