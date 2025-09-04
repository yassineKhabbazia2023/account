// <copyright file="OfferEligibilityControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Account.Api.Tests.Controllers;

public class OfferEligibilityControllerTests
{
    [Fact]
    public async Task UpdateOfferEligibilityAsync_Should_ReturnOkWithEntityUpdated()
    {
        // Arrange
        var currentUserId = 123;
        var accountId = 456;

        var expectedEntity = new OfferEligibility
        {
            AccountId = accountId,
            IsEligible = true,
            ApprovedDate = DateTime.UtcNow
        };

        var serviceMock = new Mock<IOfferEligibilityService>();
        serviceMock
            .Setup(s => s.UpdateOfferEligibilityAsync(currentUserId, accountId))
            .ReturnsAsync(expectedEntity);

        var controller = new OfferEligibilityController(serviceMock.Object);

        // Act
        var result = await controller.UpdateOfferEligibilityAsync(currentUserId, accountId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var entity = Assert.IsType<OfferEligibility>(okResult.Value);

        Assert.Equal(expectedEntity, entity);

        serviceMock.Verify(s => s.UpdateOfferEligibilityAsync(currentUserId, accountId), Times.Once);
    }
}
