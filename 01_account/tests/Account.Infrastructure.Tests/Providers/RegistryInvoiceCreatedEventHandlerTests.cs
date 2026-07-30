// <copyright file="RegistryInvoiceCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RegistryInvoiceCreatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_NewInvoice_InsertsRowAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>();

        repositoryMock
            .Setup(r => r.ExistsByInvoiceNumberAsync("INV-001"))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(r => r.GetAccountIdByAccountNumberAsync("ACC123"))
            .ReturnsAsync(5);
        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<CreateInvoiceRequest>()))
            .Returns(Task.CompletedTask);

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\",\"AccountNumber\":\"ACC123\",\"Name\":\"Invoice 001\",\"InvoiceDate\":\"2024-01-15T00:00:00Z\",\"DepositDate\":\"2024-01-16T00:00:00Z\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync("INV-001"), Times.Once);
        repositoryMock.Verify(r => r.GetAccountIdByAccountNumberAsync("ACC123"), Times.Once);
        repositoryMock.Verify(
            r => r.AddAsync(It.Is<CreateInvoiceRequest>(req =>
                req.InvoiceNumber == "INV-001" &&
                req.Name == "Invoice 001" &&
                req.AccountId == 5)),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExistingInvoiceNumber_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        repositoryMock
            .Setup(r => r.ExistsByInvoiceNumberAsync("INV-001"))
            .ReturnsAsync(true);

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\",\"AccountNumber\":\"ACC123\",\"Name\":\"Invoice 001\",\"InvoiceDate\":\"2024-01-15T00:00:00Z\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync("INV-001"), Times.Once);
        repositoryMock.Verify(r => r.GetAccountIdByAccountNumberAsync(It.IsAny<string>()), Times.Never);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<CreateInvoiceRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UnknownAccountNumber_LogsWarningAndAcksAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        repositoryMock
            .Setup(r => r.ExistsByInvoiceNumberAsync("INV-001"))
            .ReturnsAsync(false);
        repositoryMock
            .Setup(r => r.GetAccountIdByAccountNumberAsync("UNKNOWN"))
            .ReturnsAsync((int?)null);

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\",\"AccountNumber\":\"UNKNOWN\",\"Name\":\"Invoice 001\",\"InvoiceDate\":\"2024-01-15T00:00:00Z\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync("INV-001"), Times.Once);
        repositoryMock.Verify(r => r.GetAccountIdByAccountNumberAsync("UNKNOWN"), Times.Once);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<CreateInvoiceRequest>()), Times.Never);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMissingData_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMissingInvoiceNumber_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\",\"Data\":{\"AccountNumber\":\"ACC123\",\"Name\":\"Invoice 001\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ThrowsJsonExceptionAndLogsErrorAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);
        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => handler.HandleAsync("{"));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => (state.ToString() ?? string.Empty).Contains("Erreur inattendue dans RegistryInvoiceCreatedEventHandler.HandleAsync")),
                It.IsAny<JsonException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyAccountNumber_LogsErrorAndReturnsAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);
        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\",\"AccountNumber\":\"  \",\"Name\":\"Invoice 001\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.ExistsByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => (state.ToString() ?? string.Empty).Contains("AccountNumber is null or empty")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_LogsAndRethrowsAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceCreatedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>();
        repositoryMock
            .Setup(r => r.ExistsByInvoiceNumberAsync("INV-001"))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new RegistryInvoiceCreatedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceCreatedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\",\"AccountNumber\":\"ACC123\",\"Name\":\"Invoice 001\"}}";

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(message));

        // Assert
        Assert.Equal("boom", exception.Message);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => (state.ToString() ?? string.Empty).Contains("Erreur inattendue dans RegistryInvoiceCreatedEventHandler.HandleAsync")),
                It.IsAny<InvalidOperationException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}
