// <copyright file="VentyaServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services;

public class VentyaServiceTests
{
    private readonly Mock<IVentyaRepository> _ventyaRepository;
    private readonly Mock<ILogger<VentyaService>> _logger;

    public VentyaServiceTests()
    {
        _ventyaRepository = new Mock<IVentyaRepository>();
        _logger = new Mock<ILogger<VentyaService>>();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_WithAccess_ShouldReturnTrue()
    {
        // Arrange
        var result = new VentyaAccessResult
        {
            AccountFound = true,
            ContactFound = true,
            RoleFound = true,
            HasAccess = true
        };
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync("ACC001", 123);

        // Assert
        hasAccess.Should().BeTrue();
        _ventyaRepository.Verify(x => x.CheckVentyaAccessAsync("ACC001", 123), Times.Once);
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_WithoutAccess_ShouldReturnFalse()
    {
        // Arrange
        var result = new VentyaAccessResult
        {
            AccountFound = true,
            ContactFound = true,
            RoleFound = true,
            HasAccess = false
        };
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync("ACC001", 123);

        // Assert
        hasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_AccountNotFound_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var result = new VentyaAccessResult
        {
            AccountFound = false,
            ContactFound = false,
            RoleFound = false,
            HasAccess = false
        };
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync("UNKNOWN", 123);

        // Assert
        hasAccess.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Account with number 'UNKNOWN' not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_ContactNotFound_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var result = new VentyaAccessResult
        {
            AccountFound = true,
            ContactFound = false,
            RoleFound = false,
            HasAccess = false
        };
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync("ACC001", 999);

        // Assert
        hasAccess.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Contact with id '999' not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_RoleNotFound_ShouldReturnFalseAndLogWarning()
    {
        // Arrange
        var result = new VentyaAccessResult
        {
            AccountFound = true,
            ContactFound = true,
            RoleFound = false,
            HasAccess = false
        };
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync("ACC001", 123);

        // Assert
        hasAccess.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Role for contact '123' on account 'ACC001' not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckVentyaAccessAsync_ContactIsNotCustomer_ShouldReturnFalseWithoutLogging()
    {
        // Arrange
        var result = new VentyaAccessResult
        {
            AccountFound = true,
            ContactFound = true,
            RoleFound = false,
            HasAccess = false
        };
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync("ACC001", 123);

        // Assert
        hasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountNumberIsEmpty_ReturnsNotFound()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(" ");

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        repository.Verify(r => r.GetAccountEmailAsync(It.IsAny<string>()), Times.Never);
        repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountNumberIsNull_ReturnsNotFound()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(null!);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        repository.Verify(r => r.GetAccountEmailAsync(It.IsAny<string>()), Times.Never);
        repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
            .ReturnsAsync((AccountExists: false, AccountEmail: (string?)null));

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync("ACC123");

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountEmailMissing_ReturnsIsReadyFalse()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
            .ReturnsAsync((AccountExists: true, AccountEmail: (string?)null));
        repository.Setup(r => r.GetSsoContactIdAsync("ACC123"))
            .ReturnsAsync(12);
        repository.Setup(r => r.GetVentyaAccessContactEmailAsync("ACC123"))
            .ReturnsAsync("contact@test.fr");

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync("ACC123");

        // Assert
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.False(result.Value!.IsReady);
        Assert.Null(result.Value.ExternalDematMail);
        Assert.Equal("contact@test.fr", result.Value.ContactWithAccess);
        repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetVentyaAccessContactEmailAsync("ACC123"), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenSsoContactMissing_ReturnsIsReadyFalse()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
            .ReturnsAsync((true, "account@test.fr"));
        repository.Setup(r => r.GetSsoContactIdAsync("ACC123"))
            .ReturnsAsync((int?)null);
        repository.Setup(r => r.GetVentyaAccessContactEmailAsync("ACC123"))
            .ReturnsAsync((string?)null);

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync("ACC123");

        // Assert
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.False(result.Value!.IsReady);
        Assert.Equal("account@test.fr", result.Value.ExternalDematMail);
        Assert.Null(result.Value.ContactWithAccess);
        repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetVentyaAccessContactEmailAsync("ACC123"), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAllCriteriaMet_ReturnsIsReadyTrue()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
            .ReturnsAsync((true, "account@test.fr"));
        repository.Setup(r => r.GetSsoContactIdAsync("ACC123"))
            .ReturnsAsync(34);
        repository.Setup(r => r.GetVentyaAccessContactEmailAsync("ACC123"))
            .ReturnsAsync("contact@test.fr");

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync("ACC123");

        // Assert
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.True(result.Value!.IsReady);
        Assert.Equal("account@test.fr", result.Value.ExternalDematMail);
        Assert.Equal("contact@test.fr", result.Value.ContactWithAccess);
        repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync("ACC123"), Times.Once);
        repository.Verify(r => r.GetVentyaAccessContactEmailAsync("ACC123"), Times.Once);
    }
}
