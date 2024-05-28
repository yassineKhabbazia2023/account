// <copyright file="RoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Repositories;

public class RoleEventRepository : IRoleEventRepository
{
    private readonly AccountContext _accountContext;

    public RoleEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();
    }

    public async Task DeleteContactRolesAsync(int contactId)
    {
        await _accountContext.RoleEntity.Where(r => r.ContactId == contactId)
                .ForEachAsync(r =>
                {
                    _accountContext.Entry(r).State = EntityState.Deleted;
                });

        await _accountContext.SaveChangesAsync();
    }
}
