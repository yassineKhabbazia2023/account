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
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync("ACC001", 123);
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as VentyaAccessResponse;

        // Assert
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response!.HasAccess.Should().BeTrue();
        response.ContactWithAccess.Should().BeNull();
        ventyaService.Verify(x => x.CheckVentyaAccessAsync("ACC001", 123), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_WithoutAccess_ShouldReturnOkWithHasAccessFalse()
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(false);
        ventyaService.Setup(service => service.GetVentyaAccessContactEmailAsync("ACC001"))
            .ReturnsAsync("toto@gmail.com");

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync("ACC001", 456);
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as VentyaAccessResponse;

        // Assert
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response!.HasAccess.Should().BeFalse();
        response.ContactWithAccess.Should().Be("toto@gmail.com");
        ventyaService.Verify(x => x.CheckVentyaAccessAsync("ACC001", 456), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync("ACC001"), Times.Once);
    }

    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        const string accountNumber = "TEST123";
        const int contactId = 789;

        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(accountNumber, contactId))
            .ReturnsAsync(true);

        var controller = new VentyaController(ventyaService.Object);

        // Act
        await controller.CheckCurrentUserVentyaAccessAsync(accountNumber, contactId);

        // Assert
        ventyaService.Verify(x => x.CheckVentyaAccessAsync(accountNumber, contactId), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckCurrentUserVentyaAccessAsync_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var controller = new VentyaController(ventyaService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.CheckCurrentUserVentyaAccessAsync("ACC001", 123));
    }

    [Theory]
    [InlineData("ACC001", 1)]
    [InlineData("999999", 99999)]
    [InlineData("A", 0)]
    public async Task CheckCurrentUserVentyaAccessAsync_WithVariousInputs_ShouldPassParametersToService(string accountNumber, int contactId)
    {
        // Arrange
        var ventyaService = new Mock<IVentyaService>(MockBehavior.Strict);
        ventyaService.Setup(service => service.CheckVentyaAccessAsync(accountNumber, contactId))
            .ReturnsAsync(false);
        ventyaService.Setup(service => service.GetVentyaAccessContactEmailAsync(accountNumber))
            .ReturnsAsync("contact@test.local");

        var controller = new VentyaController(ventyaService.Object);

        // Act
        var result = await controller.CheckCurrentUserVentyaAccessAsync(accountNumber, contactId);
        var okResult = result.Result as OkObjectResult;

        // Assert
        okResult.Should().NotBeNull();
        ventyaService.Verify(x => x.CheckVentyaAccessAsync(accountNumber, contactId), Times.Once);
        ventyaService.Verify(x => x.GetVentyaAccessContactEmailAsync(accountNumber), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenAccountNotFound_ReturnsNotFound()
    {
        // Arrange
        var accountNumber = "ACC123";
        var serviceMock = new Mock<IVentyaService>();
        serviceMock.Setup(s => s.CheckAccountIsDematReadyAsync(accountNumber))
            .ReturnsAsync(Result<DematReadyResponse>.NotFound());

        var controller = new VentyaController(serviceMock.Object);

        // Act
        var result = await controller.CheckAccountIsDematReadyAsync(accountNumber);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        serviceMock.Verify(s => s.CheckAccountIsDematReadyAsync(accountNumber), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenReady_ReturnsOkWithIsReadyTrue()
    {
        // Arrange
        var accountNumber = "ACC123";
        var response = new DematReadyResponse { IsReady = true };
        var serviceMock = new Mock<IVentyaService>();
        serviceMock.Setup(s => s.CheckAccountIsDematReadyAsync(accountNumber))
            .ReturnsAsync(Result<DematReadyResponse>.Success(response));

        var controller = new VentyaController(serviceMock.Object);

        // Act
        var result = await controller.CheckAccountIsDematReadyAsync(accountNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DematReadyResponse>(okResult.Value);
        Assert.True(payload.IsReady);
        serviceMock.Verify(s => s.CheckAccountIsDematReadyAsync(accountNumber), Times.Once);
    }

    [Fact]
    public async Task CheckAccountIsDematReadyAsync_WhenNotReady_ReturnsOkWithIsReadyFalse()
    {
        // Arrange
        var accountNumber = "ACC123";
        var response = new DematReadyResponse { IsReady = false };
        var serviceMock = new Mock<IVentyaService>();
        serviceMock.Setup(s => s.CheckAccountIsDematReadyAsync(accountNumber))
            .ReturnsAsync(Result<DematReadyResponse>.Success(response));

        var controller = new VentyaController(serviceMock.Object);

        // Act
        var result = await controller.CheckAccountIsDematReadyAsync(accountNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DematReadyResponse>(okResult.Value);
        Assert.False(payload.IsReady);
        serviceMock.Verify(s => s.CheckAccountIsDematReadyAsync(accountNumber), Times.Once);
    }
}
