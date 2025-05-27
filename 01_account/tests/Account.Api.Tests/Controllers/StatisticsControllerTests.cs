// <copyright file="StatisticsControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Account.Api.Tests.Controllers
{
    public class StatisticsControllerTests
    {
        private readonly Mock<IStatisticsService> _statisticsServiceMock;

        public StatisticsControllerTests()
        {
            _statisticsServiceMock = new Mock<IStatisticsService>(MockBehavior.Strict);
        }

        [Fact]
        public async Task GetStatistics_DefaultParam_ReturnExpected()
        {
            var expected = new Statistics
            {
                AccountConnected = 10,
                AccountToDeploy = 3,
                AccountInProgress = 5
            };

            _statisticsServiceMock.Setup(service => service.GetStatisticsAsync(It.IsAny<int>())).ReturnsAsync(expected);
            var statisticsController = new StatisticsController(_statisticsServiceMock.Object);

            var statistics = await statisticsController.GetStatistics(It.IsAny<int>());
            var result = statistics?.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public async Task GetAccountPercentageCustomerRelation_ReturnExpected()
        {
            double expected = 1;

            _statisticsServiceMock.Setup(service => service.GetAccountPercentageCustomerRelationAsync()).ReturnsAsync(expected);
            var statisticsController = new StatisticsController(_statisticsServiceMock.Object);

            var statistics = await statisticsController.GetAccountPercentageCustomerRelation();
            var result = statistics?.Result as OkObjectResult;

            Assert.Equal(expected, result!.Value);
        }
    }
}
