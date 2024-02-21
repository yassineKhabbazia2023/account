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
        public async Task GetHubsAsync_Should_Return_OkResult()
        {
            var hubs = _fixture.CreateMany<Hub>();
            _service.Setup(x => x.GetHubsAsync()).ReturnsAsync(hubs).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = await referentialController.GetHubsAsync();

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(hubs);
        }

        [Fact]
        public async Task GetNafsAsync_Should_Return_OkResult()
        {
            var nafs = _fixture.CreateMany<Naf>();
            _service.Setup(x => x.GetNafsAsync()).ReturnsAsync(nafs).Verifiable();

            var referentialController = new ReferentialController(_service.Object);

            var result = await referentialController.GetNafsAsync();

            result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
            result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(nafs);
        }
    }
}
