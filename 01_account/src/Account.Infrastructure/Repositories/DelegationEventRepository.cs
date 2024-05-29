// <copyright file="DelegationEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Providers.Interfaces;

namespace Pulse.Account.Infrastructure.Repositories;

public class DelegationEventRepository : IDelegationEventRepository
{
    private readonly AccountContext _accountContext;

    public DelegationEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
        _accountContext.HandleEFCoreFailure();
    }

    public async Task DeleteContactDelegationsAsync(int contactId)
    {
        var delegationsToDelete = await _accountContext.DelegationEntity.Where(d => d.DelegateeId == contactId).ToListAsync();
        delegationsToDelete.ForEach(d =>
        {
            d.Status = DelegationStatus.Disabled.ToString().ToLower();
        });

        await _accountContext.SaveChangesAsync();
    }
}
