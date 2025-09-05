// <copyright file="DelegationEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DelegationEventRepositoryTests
{
    [Fact]
    public async Task DeleteContactDelegationsAsync_ShouldDeleteAllContactDelegations()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new DelegationEventRepository(context);

        var delegation1 = new DelegationEntity
        {
            DelegationId = 1,
            DelegatorId = 2,
            DelegateeId = 1,
            Status = "pending"
        };

        var delegation2 = new DelegationEntity
        {
            DelegationId = 2,
            DelegatorId = 3,
            DelegateeId = 1,
            Status = "enabled"
        };

        await context.DelegationEntity.AddRangeAsync(new List<DelegationEntity> { delegation1, delegation2 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await repository.DeleteContactDelegationsAsync(1);

        var result = await context.DelegationEntity.Where(r => r.DelegateeId == 1).ToListAsync();

        result.ForEach(d =>
        {
            Assert.Equal(DelegationStatus.Disabled.ToString().ToLower(), d.Status);
        });
    }
}
