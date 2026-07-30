// <copyright file="RegistryInvoiceRemovedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class RegistryInvoiceRemovedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingInvoice_RemovesRowAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>();

        repositoryMock
            .Setup(r => r.RemoveByInvoiceNumberAsync("INV-001"))
            .Returns(Task.CompletedTask);

        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceRemovedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.RemoveByInvoiceNumberAsync("INV-001"), Times.Once);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => (state.ToString() ?? string.Empty).Contains("Facture supprimée ou ignorée")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnknownInvoice_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>();

        repositoryMock
            .Setup(r => r.RemoveByInvoiceNumberAsync("INV-999"))
            .Returns(Task.CompletedTask);

        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceRemovedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-999\"}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.RemoveByInvoiceNumberAsync("INV-999"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullMessage_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await handler.HandleAsync(null!);

        // Assert
        repositoryMock.Verify(r => r.RemoveByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMissingData_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceRemovedEvent\"}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.RemoveByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMissingInvoiceNumber_DoesNothingAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);

        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceRemovedEvent\",\"Data\":{}}";

        // Act
        await handler.HandleAsync(message);

        // Assert
        repositoryMock.Verify(r => r.RemoveByInvoiceNumberAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidJson_ThrowsAndLogsErrorAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);
        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => handler.HandleAsync("{"));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => (state.ToString() ?? string.Empty).Contains("Erreur inattendue dans RegistryInvoiceRemovedEventHandler.HandleAsync")),
                It.IsAny<JsonException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_LogsErrorAndRethrowsAsync()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<RegistryInvoiceRemovedEventHandler>>();
        var repositoryMock = new Mock<IInvoiceRepository>();
        repositoryMock
            .Setup(r => r.RemoveByInvoiceNumberAsync("INV-001"))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new RegistryInvoiceRemovedEventHandler(loggerMock.Object, repositoryMock.Object);
        var message = "{\"EventType\":\"RegistryInvoiceRemovedEvent\",\"Data\":{\"InvoiceNumber\":\"INV-001\"}}";

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(message));

        // Assert
        Assert.Equal("boom", exception.Message);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => (state.ToString() ?? string.Empty).Contains("Erreur inattendue dans RegistryInvoiceRemovedEventHandler.HandleAsync")),
                It.IsAny<InvalidOperationException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}
