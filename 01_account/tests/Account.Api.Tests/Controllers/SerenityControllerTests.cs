// <copyright file="SerenityControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Reflection;
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

    #region Eligibility

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

    #endregion

    #region Create choice

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

    #endregion

    #region Reset choice

    /// <summary>
    /// Verifies that reset forwards the URL contact identifier and returns no content.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_ShouldReturnNoContentAndForwardContactId()
    {
        // Arrange
        var controller = new SerenityController(_serenityServiceMock.Object);

        // Act
        var result = await controller.ResetSerenityChoiceAsync(CurrentUserId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _serenityServiceMock.Verify(service => service.ResetSerenityChoiceAsync(CurrentUserId), Times.Once);
    }

    /// <summary>
    /// Verifies that contactId is mandatory, route-bound, and never supplied by a body payload.
    /// </summary>
    [Fact]
    public void ResetSerenityChoiceAsync_ShouldBindValidatedContactIdFromRoute()
    {
        // Arrange
        var method = typeof(SerenityController).GetMethod(nameof(SerenityController.ResetSerenityChoiceAsync));
        Assert.NotNull(method);

        // Act
        var httpDelete = method.GetCustomAttribute<HttpDeleteAttribute>();
        var parameter = Assert.Single(method.GetParameters());

        // Assert
        Assert.Equal("serenity-choice/{contactId:int}/reset", httpDelete!.Template);
        Assert.NotNull(parameter.GetCustomAttribute<FromRouteAttribute>());
        var range = parameter.GetCustomAttribute<RangeAttribute>();
        Assert.NotNull(range);
        Assert.Equal(1, range!.Minimum);
        Assert.Equal(int.MaxValue, range.Maximum);
        Assert.Null(parameter.GetCustomAttribute<FromBodyAttribute>());
    }

    /// <summary>
    /// Verifies that service failures are left to the API exception pipeline.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_WhenServiceFails_ShouldPropagateFailure()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service failure");
        _serenityServiceMock
            .Setup(service => service.ResetSerenityChoiceAsync(CurrentUserId))
            .ThrowsAsync(expectedException);
        var controller = new SerenityController(_serenityServiceMock.Object);

        // Act
        var action = () => controller.ResetSerenityChoiceAsync(CurrentUserId);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Same(expectedException, exception);
    }

    #endregion
}
