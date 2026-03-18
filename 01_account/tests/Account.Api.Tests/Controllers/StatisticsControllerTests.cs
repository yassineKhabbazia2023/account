// <copyright file="StatisticsControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Account.Api.Tests.Controllers;

public class StatisticsControllerTests
{
    private readonly Mock<IStatisticsService> _statisticsServiceMock;

    public StatisticsControllerTests()
    {
        _statisticsServiceMock = new Mock<IStatisticsService>();
    }

    [Fact]
    public async Task GetStatistics_DefaultParam_ShouldReturnExpected()
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
    public async Task GetAccountPercentageCustomerRelation_ShouldReturnExpected()
    {
        double expected = 1;

        _statisticsServiceMock.Setup(service => service.GetAccountPercentageCustomerRelationAsync()).ReturnsAsync(expected);
        var statisticsController = new StatisticsController(_statisticsServiceMock.Object);

        var statistics = await statisticsController.GetAccountPercentageCustomerRelationAsync();
        var result = statistics?.Result as OkObjectResult;

        Assert.Equal(expected, result!.Value);
    }

    [Fact]
    public async Task GetAccountAndClientIndicatorsAsync_ShouldReturnExpected()
    {
        var excelBytes = new byte[] { 1, 2, 3, 4, 5 };
        _statisticsServiceMock.Setup(x => x.GetAccountAndClientIndicatorsAsync()).ReturnsAsync(excelBytes);

        var statisticsController = new StatisticsController(_statisticsServiceMock.Object);

        var result = await statisticsController.GetAccountAndClientIndicatorsAsync();

        Assert.NotNull(result);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Equal("AccountContactIndicators.xlsx", fileResult.FileDownloadName);
        Assert.Equal(excelBytes, fileResult.FileContents);

        _statisticsServiceMock.Verify(x => x.GetAccountAndClientIndicatorsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEntityCountByTypeAsync_ShouldReturnExpected()
    {
        var expected = new EntityCountByType
        {
            RegularEntitiesCount = 24,
            ProspectEntitiesCount = 3
        };

        _statisticsServiceMock.Setup(service => service.GetEntityCountByTypeAsync(It.IsAny<int>())).ReturnsAsync(expected);
        var statisticsController = new StatisticsController(_statisticsServiceMock.Object);

        var result = await statisticsController.GetEntityCountByTypeAsync(It.IsAny<int>());
        var okResult = result?.Result as OkObjectResult;

        Assert.NotNull(okResult);
        Assert.Equal(expected, okResult.Value);
    }
}
