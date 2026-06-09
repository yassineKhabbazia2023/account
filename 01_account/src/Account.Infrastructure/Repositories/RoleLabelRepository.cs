// <copyright file="RoleLabelRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;
using InvalidOperationException = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

namespace Pulse.Account.Infrastructure.Repositories;

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

        var contact = await _accountContext.ContactEntity.AsNoTracking().FirstOrDefaultAsync(cnt => cnt.ContactId == roleLabel.ContactId);

        if (contact is null)
        {
            throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, roleLabel.ContactId));
        }

        bool isClient = contact.Type == ContactType.Customer.ToString();

        if (isClient)
        {
            throw new InvalidOperationException(Errors.NoClientLabelCode, Errors.NoClientLabelMessage);
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

    public async Task<bool> HasRoleLabel(int contactId, int accountId, int labelId)
    {
        return await _accountContext.RoleLabelEntity.AnyAsync(rl => rl.ContactId == contactId && rl.AccountId == accountId && rl.LabelId == labelId);
    }

    public async Task RemoveLabelAssignmentFromAccountAsync(int accountId, int labelId)
    {
        var existing = await _accountContext.RoleLabelEntity
            .FirstOrDefaultAsync(rl => rl.AccountId == accountId && rl.LabelId == labelId);

        if (existing is not null)
        {
            _accountContext.RoleLabelEntity.Remove(existing);
            await _accountContext.SaveChangesAsync();
        }
    }

    public async Task<string?> GetLabelCodeAsync(int labelId)
    {
        return await _accountContext.LabelEntity
            .AsNoTracking()
            .Where(l => l.LabelId == labelId)
            .Select(l => l.Code)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasExclusiveLabelAsync(int accountId, int contactId)
    {
        return await _accountContext.RoleLabelEntity
            .AsNoTracking()
            .AnyAsync(rl => rl.AccountId == accountId && rl.ContactId == contactId
                && (rl.Label.Code == RoleLabelCodes.AccountManager || rl.Label.Code == RoleLabelCodes.CustomerLeadPartner));
    }
}
