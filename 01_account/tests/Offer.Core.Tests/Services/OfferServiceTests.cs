// <copyright file="OfferServiceTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Kpmg.Offer.Core.Exceptions;
using Kpmg.Offer.Core.Interfaces;
using Kpmg.Offer.Core.Services;
using Moq;
using OfferModel = Kpmg.Offer.Core.Models.Offer;

namespace Kpmg.Offer.Core.Tests.Services
{
    public class OfferServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IOfferRepository> _offerRepository;
        private readonly OfferService _offerService;

        public OfferServiceTests()
        {
            _fixture = new Fixture();
            _offerRepository = new Mock<IOfferRepository>();
            _offerService = new OfferService(_offerRepository.Object);
        }

        [Fact]
        public async Task Should_GetOffersAsync_Nominal()
        {
            var expected = _fixture.Create<List<OfferModel>>();

            _offerRepository.Setup(x => x.GetOffersAsync()).ReturnsAsync(expected);
            var result = await _offerService.GetOffersAsync();

            Assert.NotNull(result);
            result.Should().BeEquivalentTo(expected);
            _offerRepository.Verify(x => x.GetOffersAsync(), Times.Once);
        }

        [Fact]
        public async Task Should_GetOffersAsync_Return_Nothig_when_empty_Offers()
        {
            var expected = new List<OfferModel>();

            _offerRepository.Setup(x => x.GetOffersAsync()).ReturnsAsync(expected);
            var result = await _offerService.GetOffersAsync();

            Assert.Empty(result);
            _offerRepository.Verify(x => x.GetOffersAsync(), Times.Once);
        }

        [Fact]
        public async Task Should_GetOfferByIdAsync_Nominal()
        {
            var expected = _fixture.Create<OfferModel>();
            _offerRepository.Setup(x => x.GetOfferByIdAsync(It.IsAny<int>())).ReturnsAsync(expected);
            var result = await _offerService.GetOfferByIdAsync(expected.OfferId);

            Assert.NotNull(result);
            result.Should().BeEquivalentTo(expected);
            _offerRepository.Verify(x => x.GetOfferByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Should_GetOfferByIdAsync_Return_Exception_When_NotFound_OfferId()
        {
            // Arrange
            _offerRepository.Setup(service => service.GetOfferByIdAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(Errors.NotFoundOfferCode, Errors.NotFoundOfferMessage));

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _offerService.GetOfferByIdAsync(It.IsAny<int>()));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(Errors.NotFoundOfferCode, exception.Code);
            Assert.Equal(Errors.NotFoundOfferMessage, exception.Message);
        }
    }
}
