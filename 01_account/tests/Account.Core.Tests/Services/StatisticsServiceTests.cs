// <copyright file="StatisticsServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text;
using ClosedXML.Excel;
using FluentAssertions;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services;

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

    [Fact]
    public async Task GetAccountAndClientIndicatorsAsync_ShouldCreateExcelFile()
    {
        var clientsPerAccount = new List<(int ClientCount, int AccountCount)>
        {
            (100, 1),
            (50, 2)
        };
        var accountsPerClient = new List<(int AccountCount, int ClientCount)>
        {
            (17, 1),
            (25, 2)
        };

        _statisticsRepositoryMock.Setup(x => x.GetAccountsPerClientCountAsync()).ReturnsAsync(accountsPerClient);
        _statisticsRepositoryMock.Setup(x => x.GetClientsPerAccountCountAsync()).ReturnsAsync(clientsPerAccount);

        var service = new StatisticsService(_statisticsRepositoryMock.Object);

        var result = await service.GetAccountAndClientIndicatorsAsync();

        Assert.NotNull(result);

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);

        // Vérifie les noms des feuilles
        Assert.True(workbook.Worksheets.Count >= 2);
        var ws1 = workbook.Worksheet("Clients par dossiers");
        var ws2 = workbook.Worksheet("Entités par clients");
        Assert.NotNull(ws1);
        Assert.NotNull(ws2);

        var header1 = ws1.RangeUsed().FirstRow().Cells().Select(c => c.Value.ToString()).ToList();
        Assert.Contains("Nombre de dossiers", header1);
        Assert.Contains("Nombre de clients", header1);
        Assert.Contains("Description", header1);

        var data1 = ws1.RangeUsed().RowsUsed().Skip(1).ToList();
        Assert.Equal(2, data1.Count);
        Assert.Equal("1", data1[0].Cell(1).GetString());
        Assert.Equal("100", data1[0].Cell(2).GetString());
        Assert.Contains("1 dossier(s) ont 100 client(s) rattaché(s)", data1[0].Cell(3).GetString());

        var header2 = ws2.RangeUsed().FirstRow().Cells().Select(c => c.Value.ToString()).ToList();
        Assert.Contains("Nombre de clients", header2);
        Assert.Contains("Nombre d'entités", header2);
        Assert.Contains("Description", header2);

        var data2 = ws2.RangeUsed().RowsUsed().Skip(1).ToList();
        Assert.Equal(2, data2.Count);
        Assert.Equal("1", data2[0].Cell(1).GetString());
        Assert.Equal("17", data2[0].Cell(2).GetString());
        Assert.Contains("1 utilisateur(s) ont 17 entité(s)", data2[0].Cell(3).GetString());

        _statisticsRepositoryMock.Verify(x => x.GetAccountsPerClientCountAsync(), Times.Once);
        _statisticsRepositoryMock.Verify(x => x.GetClientsPerAccountCountAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEntityCountByTypeAsync_ReturnExpected()
    {
        // Arrange
        var expected = new EntityCountByType
        {
            RegularEntitiesCount = 24,
            ProspectEntitiesCount = 3
        };

        _statisticsRepositoryMock.Setup(r => r.GetEntityCountByTypeAsync(It.IsAny<int>())).ReturnsAsync(expected);
        var statisticsService = new StatisticsService(_statisticsRepositoryMock.Object);

        // Act
        var result = await statisticsService.GetEntityCountByTypeAsync(It.IsAny<int>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result);
    }
}
