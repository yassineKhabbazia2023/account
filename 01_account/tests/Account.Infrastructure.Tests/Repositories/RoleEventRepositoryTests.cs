// <copyright file="RoleEventRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class RoleEventRepositoryTests
{
    [Fact]
    public async Task DeleteContactRolesAsync_ShouldDeleteAllContactRoles()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

        using var context = new AccountContext(options);
        var repository = new RoleEventRepository(context);
        var role1 = new RoleEntity
        {
            ContactId = 1,
            AccountId = 2,
            IsDelegation = true,
            IsFavorite = true,
            IsSignatory = true
        };
        var role2 = new RoleEntity
        {
            ContactId = 1,
            AccountId = 4,
            IsDelegation = false,
            IsFavorite = false,
            IsSignatory = true
        };

        await context.RoleEntity.AddRangeAsync(new List<RoleEntity> { role1, role2 });
        await context.SaveChangesAsync();

        await repository.DeleteContactRolesAsync(1);

        var result = await context.RoleEntity.Where(r => r.ContactId == 1).ToListAsync();

        Assert.Empty(result);
    }
}
