// <copyright file="ReferentialServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class ReferentialServiceTests
    {
        private readonly Mock<IReferentialRepository> _repositoryMock;
        private readonly Fixture _fixture;

        public ReferentialServiceTests()
        {
            _repositoryMock = new Mock<IReferentialRepository>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetHubsAsync_Should_Return_HubList()
        {
            var hubs = _fixture.CreateMany<Hub>();
            _repositoryMock.Setup(x => x.GetHubsAsync()).ReturnsAsync(hubs);

            var service = new ReferentialService(_repositoryMock.Object);

            var result = await service.GetHubsAsync();

            Assert.NotNull(result);
            Assert.Equal(hubs, result);
        }

        [Fact]
        public async Task GetNafsAsync_Should_Return_NafList()
        {
            var nafs = _fixture.CreateMany<Naf>();
            _repositoryMock.Setup(x => x.GetNafsAsync()).ReturnsAsync(nafs);

            var service = new ReferentialService(_repositoryMock.Object);

            var result = await service.GetNafsAsync();

            Assert.NotNull(result);
            Assert.Equal(nafs, result);
        }
    }
}
