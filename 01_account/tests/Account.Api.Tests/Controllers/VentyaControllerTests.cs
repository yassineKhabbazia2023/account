// <copyright file="VentyaControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

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
