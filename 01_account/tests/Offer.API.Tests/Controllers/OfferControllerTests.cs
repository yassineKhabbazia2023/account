// <copyright file="OfferControllerTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Kpmg.Offer.API.Controllers;
using Kpmg.Offer.Core.Exceptions;
using Kpmg.Offer.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OfferModel = Kpmg.Offer.Core.Models.Offer;

namespace Kpmg.Offer.API.Tests.Controllers
{
    public class OfferControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IOfferService> _offerService;
        private readonly OfferController _offerController;

        public OfferControllerTests()
        {
            _fixture = new Fixture();
            _offerService = new Mock<IOfferService>();
            _offerController = new OfferController(_offerService.Object);
        }

        [Fact]
        public async Task GetOffersAsync_Returns_OkResult_With_Offers()
        {
            // Arrange
            var expected = _fixture.Create<List<OfferModel>>(); // Utilise Autofixture pour créer une liste d'offres fictives
            _offerService.Setup(x => x.GetOffersAsync()).ReturnsAsync(expected);

            // Act
            var result = await _offerController.GetOffersAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var offers = Assert.IsAssignableFrom<IReadOnlyCollection<OfferModel>>(okResult.Value);
            Assert.Equal(expected, offers);
        }

        [Fact]
        public async Task GetOfferByIdAsync_With_Valid_OfferId_Returns_OkResult_With_Offer()
        {
            // Arrange
            var expected = _fixture.Create<OfferModel>();
            _offerService.Setup(x => x.GetOfferByIdAsync(It.IsAny<int>())).ReturnsAsync(expected);
            var controller = new OfferController(_offerService.Object);

            // Act
            var result = await controller.GetOfferByIdAsync(expected.OfferId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var offer = Assert.IsType<OfferModel>(okResult.Value);
            Assert.Equal(expected, offer);
        }

        [Fact]
        public async Task GetOfferByIdAsync_With_Invalid_OfferId_Returns_NotFoundException()
        {
            // Arrange
            _offerService.Setup(service => service.GetOfferByIdAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(Errors.NotFoundOfferCode, Errors.NotFoundOfferMessage));

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _offerController.GetOfferByIdAsync(It.IsAny<int>()));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(Errors.NotFoundOfferCode, exception.Code);
            Assert.Equal(Errors.NotFoundOfferMessage, exception.Message);
        }
    }
}
