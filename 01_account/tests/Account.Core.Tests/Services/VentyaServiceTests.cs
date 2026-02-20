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
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(1, 123);

        // Assert
        hasAccess.Should().BeTrue();
        _ventyaRepository.Verify(x => x.CheckVentyaAccessAsync(1, 123), Times.Once);
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
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(1, 123);

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
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(999, 123);

        // Assert
        hasAccess.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Account with id '999' not found")),
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
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(1, 999);

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
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(1, 123);

        // Assert
        hasAccess.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Role for contact '123' on account id '1' not found")),
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
        _ventyaRepository.Setup(repo => repo.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(result);

        var service = new VentyaService(_ventyaRepository.Object, _logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(1, 123);

        // Assert
        hasAccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckVentyaAccessAsync_WithInvalidAccountId_ShouldReturnFalse(int invalidAccountId)
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var hasAccess = await service.CheckVentyaAccessAsync(invalidAccountId, 123);

        // Assert
        hasAccess.Should().BeFalse();
        repository.Verify(r => r.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountIdIsInvalid_ReturnsNotFound(int invalidAccountId)
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(invalidAccountId);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        repository.Verify(r => r.GetAccountEmailAsync(It.IsAny<int>()), Times.Never);
        repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync(123))
            .ReturnsAsync((AccountExists: false, AccountEmail: (string?)null));

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(123);

        // Assert
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
        repository.Verify(r => r.GetAccountEmailAsync(123), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountEmailMissing_ReturnsIsReadyFalse()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync(123))
            .ReturnsAsync((AccountExists: true, AccountEmail: (string?)null));
        repository.Setup(r => r.GetSsoContactIdAsync(123))
            .ReturnsAsync(12);
        repository.Setup(r => r.GetVentyaAccessContactEmailAsync(123))
            .ReturnsAsync("contact@test.fr");

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(123);

        // Assert
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.False(result.Value!.IsReady);
        Assert.Null(result.Value.ExternalDematMail);
        Assert.Equal("contact@test.fr", result.Value.ContactWithAccess);
        repository.Verify(r => r.GetAccountEmailAsync(123), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync(123), Times.Once);
        repository.Verify(r => r.GetVentyaAccessContactEmailAsync(123), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenSsoContactMissing_ReturnsIsReadyFalse()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync(123))
            .ReturnsAsync((true, "account@test.fr"));
        repository.Setup(r => r.GetSsoContactIdAsync(123))
            .ReturnsAsync((int?)null);
        repository.Setup(r => r.GetVentyaAccessContactEmailAsync(123))
            .ReturnsAsync((string?)null);

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(123);

        // Assert
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.False(result.Value!.IsReady);
        Assert.Equal("account@test.fr", result.Value.ExternalDematMail);
        Assert.Null(result.Value.ContactWithAccess);
        repository.Verify(r => r.GetAccountEmailAsync(123), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync(123), Times.Once);
        repository.Verify(r => r.GetVentyaAccessContactEmailAsync(123), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAllCriteriaMet_ReturnsIsReadyTrue()
    {
        // Arrange
        var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetAccountEmailAsync(123))
            .ReturnsAsync((true, "account@test.fr"));
        repository.Setup(r => r.GetSsoContactIdAsync(123))
            .ReturnsAsync(34);
        repository.Setup(r => r.GetVentyaAccessContactEmailAsync(123))
            .ReturnsAsync("contact@test.fr");

        var logger = new Mock<ILogger<VentyaService>>();
        var service = new VentyaService(repository.Object, logger.Object);

        // Act
        var result = await service.CheckAccountIsDematReadyAsync(123);

        // Assert
        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.True(result.Value!.IsReady);
        Assert.Equal("account@test.fr", result.Value.ExternalDematMail);
        Assert.Equal("contact@test.fr", result.Value.ContactWithAccess);
        repository.Verify(r => r.GetAccountEmailAsync(123), Times.Once);
        repository.Verify(r => r.GetSsoContactIdAsync(123), Times.Once);
        repository.Verify(r => r.GetVentyaAccessContactEmailAsync(123), Times.Once);
    }
}
