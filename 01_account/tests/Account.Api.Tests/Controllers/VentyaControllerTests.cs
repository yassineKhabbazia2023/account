// <copyright file="VentyaControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Account.Api.Tests.Controllers;

public class VentyaControllerTests
{
    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_WithAccess_ShouldReturnOkWithHasAccessTrue()
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync(1, 123);
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as VentyaAccessResponse;

        // Assert
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response!.HasAccess.Should().BeTrue();
        response.ContactWithAccess.Should().BeNull();
        ventyaService.Verify(x => x.CheckVentyaAccessAsync(1, 123), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_WithoutAccess_ShouldReturnOkWithHasAccessFalse()
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);
        ventyaService.Setup(service => service.GetVentyaAccessContactEmailAsync(1))
            .ReturnsAsync("toto@gmail.com");

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync(1, 456);
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as VentyaAccessResponse;

        // Assert
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response!.HasAccess.Should().BeFalse();
        response.ContactWithAccess.Should().Be("toto@gmail.com");
        ventyaService.Verify(x => x.CheckVentyaAccessAsync(1, 456), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(1), Times.Once);
    }

    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 789;

        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(accountId, contactId))
            .ReturnsAsync(true);

        var controller = new VentyaController(ventyaService.Object);

        // Act
        await controller.CheckCurrentUserVentyaAccessAsync(accountId, contactId);

        // Assert
        ventyaService.Verify(x => x.CheckVentyaAccessAsync(accountId, contactId), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var controller = new VentyaController(ventyaService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.CheckCurrentUserVentyaAccessAsync(1, 123));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(999999, 99999)]
    [InlineData(100, 0)]
    public async Task CheckCurrentUserVentyaAccessAsync_WithVariousInputs_ShouldPassParametersToService(int accountId, int contactId)
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(accountId, contactId))
            .ReturnsAsync(false);
        ventyaService.Setup(service => service.GetVentyaAccessContactEmailAsync(accountId))
            .ReturnsAsync("contact@test.local");

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync(accountId, contactId);
        var okResult = result.Result as OkObjectResult;

        // Assert
        okResult.Should().NotBeNull();
        ventyaService.Verify(x => x.CheckVentyaAccessAsync(accountId, contactId), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(accountId), Times.Once);
    }

    [Theory]
    [InlineData(0, 123)]
    [InlineData(-1, 123)]
    public async Task CheckCurrentUserVentyaAccessAsync_WithInvalidAccountId_ShouldReturnHasAccessFalse(int accountId, int contactId)
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(accountId, contactId))
            .ReturnsAsync(false);
        ventyaService.Setup(service => service.GetVentyaAccessContactEmailAsync(accountId))
            .ReturnsAsync((string?)null);

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync(accountId, contactId);
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as VentyaAccessResponse;

        // Assert
        okResult.Should().NotBeNull();
        response.Should().NotBeNull();
        response!.HasAccess.Should().BeFalse();
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountNotFound_ReturnsNotFound()
    {
        // Arrange
        var accountId = 123;
        var serviceMock = new Mock<IVentyaService>();
        serviceMock.Setup(s => s.CheckAccountIsDematReadyAsync(accountId))
            .ReturnsAsync(Result<DematReadyResponse>.NotFound());

        var controller = new VentyaController(serviceMock.Object);

        // Act
        var result = await controller.CheckAccountIsDematReadyAsync(accountId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        serviceMock.Verify(s => s.CheckAccountIsDematReadyAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenReady_ReturnsOkWithIsReadyTrue()
    {
        // Arrange
        var accountId = 123;
        var response = new DematReadyResponse { IsReady = true };
        var serviceMock = new Mock<IVentyaService>();
        serviceMock.Setup(s => s.CheckAccountIsDematReadyAsync(accountId))
            .ReturnsAsync(Result<DematReadyResponse>.Success(response));

        var controller = new VentyaController(serviceMock.Object);

        // Act
        var result = await controller.CheckAccountIsDematReadyAsync(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DematReadyResponse>(okResult.Value);
        Assert.True(payload.IsReady);
        serviceMock.Verify(s => s.CheckAccountIsDematReadyAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenNotReady_ReturnsOkWithIsReadyFalse()
    {
        // Arrange
        var accountId = 123;
        var response = new DematReadyResponse { IsReady = false };
        var serviceMock = new Mock<IVentyaService>();
        serviceMock.Setup(s => s.CheckAccountIsDematReadyAsync(accountId))
            .ReturnsAsync(Result<DematReadyResponse>.Success(response));

        var controller = new VentyaController(serviceMock.Object);

        // Act
        var result = await controller.CheckAccountIsDematReadyAsync(accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DematReadyResponse>(okResult.Value);
        Assert.False(payload.IsReady);
        serviceMock.Verify(s => s.CheckAccountIsDematReadyAsync(accountId), Times.Once);
    }
}
