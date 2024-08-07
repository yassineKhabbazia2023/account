// <copyright file="AccountRepositoryOptionTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Tests.Repositories;
using System;
using Xunit;
using FluentAssertions;

public class AccountRepositoryOptionsTests
{
    [Fact]
    public void ConnectionString_ShouldBeSettableAndGettable()
    {
        // Arrange
        var options = new AccountRepositoryOptions();
        var connectionString = "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;";

        // Act
        options.ConnectionString = connectionString;

        // Assert
        options.ConnectionString.Should().Be(connectionString);
    }

    [Fact]
    public void Validate_WithValidConnectionString_ShouldNotThrow()
    {
        // Arrange
        var options = new AccountRepositoryOptions
        {
            ConnectionString = "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;"
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new AccountRepositoryOptions
        {
            ConnectionString = null
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
               .Should().Throw<InvalidOperationException>()
               .WithMessage("Instance of AccountRepositoryOptions is invalid, ConnectionString is null or empty.");
    }

    [Fact]
    public void Validate_WithEmptyConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new AccountRepositoryOptions
        {
            ConnectionString = ""
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
               .Should().Throw<InvalidOperationException>()
               .WithMessage("Instance of AccountRepositoryOptions is invalid, ConnectionString is null or empty.");
    }

    [Fact]
    public void Validate_WithWhitespaceConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new AccountRepositoryOptions
        {
            ConnectionString = "   "
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
               .Should().Throw<InvalidOperationException>()
               .WithMessage("Instance of AccountRepositoryOptions is invalid, ConnectionString is null or empty.");
    }
}
