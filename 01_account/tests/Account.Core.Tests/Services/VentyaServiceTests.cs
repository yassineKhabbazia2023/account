// <copyright file="VentyaServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class VentyaServiceTests
    {
        [Fact]
        public async Task CheckAccountIsDematReadyAsync_WhenAccountNumberIsEmpty_ReturnsNotFound()
        {
            // Arrange
            var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
            var service = new VentyaService(repository.Object);

            // Act
            var result = await service.CheckAccountIsDematReadyAsync(" ");

            // Assert
            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            repository.Verify(r => r.GetAccountEmailAsync(It.IsAny<string>()), Times.Never);
            repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CheckAccountIsDematReadyAsync_WhenAccountNumberIsNull_ReturnsNotFound()
        {
            // Arrange
            var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
            var service = new VentyaService(repository.Object);

            // Act
            var result = await service.CheckAccountIsDematReadyAsync(null!);

            // Assert
            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            repository.Verify(r => r.GetAccountEmailAsync(It.IsAny<string>()), Times.Never);
            repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CheckAccountIsDematReadyAsync_WhenAccountNotFound_ReturnsNotFound()
        {
            // Arrange
            var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
            repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
                .ReturnsAsync((AccountExists: false, AccountEmail: (string?)null));

            var service = new VentyaService(repository.Object);

            // Act
            var result = await service.CheckAccountIsDematReadyAsync("ACC123");

            // Assert
            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
            repository.Verify(r => r.GetSsoContactIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CheckAccountIsDematReadyAsync_WhenAccountEmailMissing_ReturnsIsReadyFalse()
        {
            // Arrange
            var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
            repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
                .ReturnsAsync((AccountExists: true, AccountEmail: (string?)null));
            repository.Setup(r => r.GetSsoContactIdAsync("ACC123"))
                .ReturnsAsync(12);

            var service = new VentyaService(repository.Object);

            // Act
            var result = await service.CheckAccountIsDematReadyAsync("ACC123");

            // Assert
            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.False(result.Value!.IsReady);
            repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
            repository.Verify(r => r.GetSsoContactIdAsync("ACC123"), Times.Once);
        }

        [Fact]
        public async Task CheckAccountIsDematReadyAsync_WhenSsoContactMissing_ReturnsIsReadyFalse()
        {
            // Arrange
            var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
            repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
                .ReturnsAsync((true, "account@test.fr"));
            repository.Setup(r => r.GetSsoContactIdAsync("ACC123"))
                .ReturnsAsync((int?)null);

            var service = new VentyaService(repository.Object);

            // Act
            var result = await service.CheckAccountIsDematReadyAsync("ACC123");

            // Assert
            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.False(result.Value!.IsReady);
            repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
            repository.Verify(r => r.GetSsoContactIdAsync("ACC123"), Times.Once);
        }

        [Fact]
        public async Task CheckAccountIsDematReadyAsync_WhenAllCriteriaMet_ReturnsIsReadyTrue()
        {
            // Arrange
            var repository = new Mock<IVentyaRepository>(MockBehavior.Strict);
            repository.Setup(r => r.GetAccountEmailAsync("ACC123"))
                .ReturnsAsync((true, "account@test.fr"));
            repository.Setup(r => r.GetSsoContactIdAsync("ACC123"))
                .ReturnsAsync(34);

            var service = new VentyaService(repository.Object);

            // Act
            var result = await service.CheckAccountIsDematReadyAsync("ACC123");

            // Assert
            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.True(result.Value!.IsReady);
            repository.Verify(r => r.GetAccountEmailAsync("ACC123"), Times.Once);
            repository.Verify(r => r.GetSsoContactIdAsync("ACC123"), Times.Once);
        }

    }
}
