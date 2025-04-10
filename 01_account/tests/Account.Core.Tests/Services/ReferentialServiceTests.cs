// <copyright file="ReferentialServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
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
        public async Task GetHubsAsync_ShouldReturnHubList()
        {
            var hubs = _fixture.CreateMany<Hub>();
            _repositoryMock.Setup(x => x.GetHubsAsync()).ReturnsAsync(hubs);

            var service = new ReferentialService(_repositoryMock.Object);

            var result = await service.GetHubsAsync();

            Assert.NotNull(result);
            Assert.Equal(hubs, result);
        }

        [Fact]
        public async Task GetNafsAsync_ShouldReturnNafList()
        {
            var nafs = _fixture.Create<Paging<Naf>>();
            _repositoryMock.Setup(x => x.GetNafsAsync(It.IsAny<string?>(), It.IsAny<Pagination>())).ReturnsAsync(nafs);

            var service = new ReferentialService(_repositoryMock.Object);

            var result = await service.GetNafsAsync(It.IsAny<string?>(), It.IsAny<Pagination>());

            Assert.NotNull(result);
            Assert.Equal(nafs, result);
        }

        [Fact]
        public void GetAccountReferentialInformation_ShouldReturnAccountReferenceInformation()
        {
            var expectedResult = new AccountReferentialInformation();

            var service = new ReferentialService(_repositoryMock.Object);

            var result = service.GetAccountReferentialInformation();

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetOfficesAsync_ShouldReturnOfficeList()
        {
            // Arrange
            var offices = _fixture.CreateMany<Office>();
            _repositoryMock.Setup(x => x.GetOfficesAsync()).ReturnsAsync(offices);
            var service = new ReferentialService(_repositoryMock.Object);

            // Act
            var result = await service.GetOfficesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(offices, result);
        }
    }
}
