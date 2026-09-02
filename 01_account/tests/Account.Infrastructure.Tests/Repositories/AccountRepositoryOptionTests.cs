// <copyright file="AccountRepositoryOptionTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

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

    [Theory]
    [MemberData(nameof(ConnectionString))]
    public void Validate_WithNullOrEmptyOrWithSpaceConnectionString_ShouldThrowInvalidOperationException(string connectionString)
    {
        // Arrange
        var options = new AccountRepositoryOptions
        {
            ConnectionString = connectionString
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
               .Should().Throw<NullArgumentException>()
               .WithMessage("Connection String au base de donnée est null ou vide!");
    }

    public static TheoryData<string> ConnectionString => new()
    {
        (string)null!,
        string.Empty,
        "                  "
    };
}
