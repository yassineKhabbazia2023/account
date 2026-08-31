// <copyright file="SerenityControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;

namespace Account.Api.Tests.Controllers;

public class SerenityControllerTests
{
    private const int CurrentUserId = 777;

    private readonly Mock<ISerenityService> _serenityServiceMock = new();

    [Fact]
    public async Task GetSerenityEligibilityAsync_Should_ReturnOkWithEligibility()
    {
        // Arrange
        var expected = new SerenityEligibility { HasMadeChoice = false, CandidateAccountIds = new[] { 1, 2 } };
        _serenityServiceMock
            .Setup(s => s.GetSerenityEligibilityAsync(CurrentUserId))
            .ReturnsAsync(expected);

        var controller = new SerenityController(_serenityServiceMock.Object);

        // Act
        var result = await controller.GetSerenityEligibilityAsync(CurrentUserId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, okResult.Value);

        _serenityServiceMock.Verify(s => s.GetSerenityEligibilityAsync(CurrentUserId), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateSerenityChoiceAsync_Should_ReturnNoContentAndForwardTheChoice(bool isAccepted)
    {
        // Arrange
        var controller = new SerenityController(_serenityServiceMock.Object);

        // Act
        var result = await controller.CreateSerenityChoiceAsync(CurrentUserId, new SerenityChoiceRequest { IsAccepted = isAccepted });

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serenityServiceMock.Verify(s => s.CreateSerenityChoiceAsync(CurrentUserId, isAccepted), Times.Once);
    }
}
