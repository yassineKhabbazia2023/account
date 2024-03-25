// <copyright file="ReferentialControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;

namespace Account.Api.Tests.Controllers
{
    public class ReferentialControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IReferentialService> _service;

        public ReferentialControllerTests()
        {
            _fixture = new Fixture();
            _service = new Mock<IReferentialService>();
        }

        [Fact]
        public async Task GetHubsAsync_ShouldReturnOkResult()
        {
            var hubs = _fixture.CreateMany<Hub>();
            _service.Setup(x => x.GetHubsAsync()).ReturnsAsync(hubs).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = await referentialController.GetHubsAsync();

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(hubs);
        }

        [Fact]
        public async Task GetHubsAsync_WithNoHubInBase_ShouldReturnOkResult()
        {
            _service.Setup(x => x.GetHubsAsync()).ReturnsAsync(Enumerable.Empty<Hub>()).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = await referentialController.GetHubsAsync();

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(Enumerable.Empty<Hub>());
        }

        [Fact]
        public async Task GetNafsAsync_ShouldReturnOkResult()
        {
            var nafs = _fixture.Create<Paging<Naf>>();
            _service.Setup(x => x.GetNafsAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(nafs).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = await referentialController.GetNafsAsync(string.Empty, 0, 0);

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(nafs);
        }

        [Fact]
        public async Task GetNafsAsync_WithNoNafInBase_ShouldReturnOkResult()
        {
            var expected = new Paging<Naf> { Items = Enumerable.Empty<Naf>() };
            _service.Setup(x => x.GetNafsAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expected).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = await referentialController.GetNafsAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>());

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetAccountReferentialInformation_ShouldReturnAccountReferenceInformation()
        {
            var expectedResult = new AccountReferentialInformation();
            _service.Setup(x => x.GetAccountReferentialInformation()).Returns(expectedResult).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = referentialController.GetAccountReferentialInformation();

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expectedResult);
        }
    }
}
