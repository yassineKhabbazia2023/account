// <copyright file="StatisticsServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class StatisticsServiceTests
    {
        private readonly Mock<IStatisticsRepository> _statisticsRepositoryMock;

        public StatisticsServiceTests()
        {
            _statisticsRepositoryMock = new Mock<IStatisticsRepository>(MockBehavior.Strict);
        }

        [Fact]
        public async Task GetStatisticsAsync_DefaultParam_ReturnExpected()
        {
            // Arrange
            var expected = new Statistics
            {
                AccountConnected = 10,
                AccountToDeploy = 3,
                AccountInProgress = 5
            };

            _statisticsRepositoryMock.Setup(r => r.GetStatisticsAsync(It.IsAny<int>())).ReturnsAsync(expected);
            var statisticsService = new StatisticsService(_statisticsRepositoryMock.Object);

            // Act
            var result = await statisticsService.GetStatisticsAsync(It.IsAny<int>());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetAccountPercentageCustomerRelationAsync_ReturnExpected()
        {
            // Arrange
            var expected = 1;

            _statisticsRepositoryMock.Setup(r => r.GetAccountPercentageCustomerRelationAsync()).ReturnsAsync(expected);
            var statisticsService = new StatisticsService(_statisticsRepositoryMock.Object);

            // Act
            var result = await statisticsService.GetAccountPercentageCustomerRelationAsync();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
