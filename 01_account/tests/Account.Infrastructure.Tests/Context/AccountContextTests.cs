// <copyright file="AccountContextTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Tests.Context;

public class AccountContextTests
{
    [Fact]
    public void InvoiceNumber_ModelConfiguration_MatchesRegistryLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new AccountContext(options);

        // Act
        var maxLength = context.Model.FindEntityType(typeof(InvoiceEntity))!
            .FindProperty(nameof(InvoiceEntity.InvoiceNumber))!
            .GetMaxLength();

        // Assert
        Assert.Equal(50, maxLength);
    }
}
