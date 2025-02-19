// <copyright file="TestAccountContext.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pulse.Account.Infrastructure.Context;

namespace Pulse.Account.Infrastructure.Tests.Context;

public class TestAccountContext : AccountContext
{
    public TestAccountContext(DbContextOptions<AccountContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(string)))
            {
                property.SetValueConverter(new ValueConverter<string, string>(
                    v => v.ToLower(),
                    v => v));
            }
        }
    }

}
