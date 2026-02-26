// <copyright file="RoleLabelServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class RoleLabelServiceTests
    {
        private readonly Mock<IRoleLabelRepository> _mockRoleLabelRepository;
        private readonly RoleLabelService _roleLabelService;

        public RoleLabelServiceTests()
        {
            _mockRoleLabelRepository = new Mock<IRoleLabelRepository>();
            _roleLabelService = new RoleLabelService(_mockRoleLabelRepository.Object);
        }

        [Fact]
        public async Task AddRoleLabelAsync_ValidRoleLabel_CallsRepository()
        {
            // Arrange
            var roleLabel = new RoleLabel
            {
                AccountId = 123,
                ContactId = 456,
                LabelId = 789,
                CreatedDate = DateTime.Now,
                CreatedBy = 101
            };

            _mockRoleLabelRepository.Setup(repo => repo.AddRoleLabelAsync(It.IsAny<RoleLabel>()))
                .Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.AddRoleLabelAsync(roleLabel);

            // Assert
            _mockRoleLabelRepository.Verify(repo => repo.AddRoleLabelAsync(roleLabel), Times.Once);
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var roleLabel = new RoleLabel
            {
                AccountId = 123,
                ContactId = 456,
                LabelId = 789,
                CreatedDate = DateTime.Now,
                CreatedBy = 101
            };

            var expectedException = new Exception("Repository error");

            _mockRoleLabelRepository.Setup(repo => repo.AddRoleLabelAsync(It.IsAny<RoleLabel>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _roleLabelService.AddRoleLabelAsync(roleLabel));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockRoleLabelRepository.Verify(repo => repo.AddRoleLabelAsync(roleLabel), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleLabelAsync_ValidParameters_CallsRepository()
        {
            // Arrange
            int accountId = 123;
            int contactId = 456;
            int labelId = 789;

            _mockRoleLabelRepository.Setup(repo => repo.DeleteRoleLabelAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId);

            // Assert
            _mockRoleLabelRepository.Verify(repo =>
                repo.DeleteRoleLabelAsync(accountId, contactId, labelId), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleLabelAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            int accountId = 123;
            int contactId = 456;
            int labelId = 789;

            var expectedException = new Exception("Repository error");

            _mockRoleLabelRepository.Setup(repo => repo.DeleteRoleLabelAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockRoleLabelRepository.Verify(repo =>
                repo.DeleteRoleLabelAsync(accountId, contactId, labelId), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleLabelAsync_NonExistentRoleLabel_ReturnsSuccessfully()
        {
            // Arrange
            int accountId = 123;
            int contactId = 456;
            int labelId = 789;

            _mockRoleLabelRepository.Setup(repo => repo.DeleteRoleLabelAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act - No exception should be thrown
            await _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId);

            // Assert
            _mockRoleLabelRepository.Verify(repo =>
                repo.DeleteRoleLabelAsync(accountId, contactId, labelId), Times.Once);
        }

        [Fact]
        public async Task HasRoleLabel_ShouldCallRepository()
        {
            // Arrange
            _mockRoleLabelRepository.Setup(repo => repo.HasRoleLabel(1, 2, 3))
                .ReturnsAsync(true);

            // Act
            var result = await _roleLabelService.HasRoleLabel(1, 2, 3);

            // Assert
            Assert.True(result);
            _mockRoleLabelRepository.Verify(repo => repo.HasRoleLabel(1, 2, 3), Times.Once);
        }

        [Fact]
        public async Task RevokeExclusiveLabelAsync_WhenCLPCode_ShouldCallRemoveLabelAssignment()
        {
            // Arrange
            _mockRoleLabelRepository.Setup(repo => repo.RemoveLabelAssignmentFromAccountAsync(1, 10))
                .Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.RevokeExclusiveLabelAsync(1, 10, "CLP");

            // Assert
            _mockRoleLabelRepository.Verify(repo => repo.RemoveLabelAssignmentFromAccountAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task RevokeExclusiveLabelAsync_WhenAMCode_ShouldCallRemoveLabelAssignment()
        {
            // Arrange
            _mockRoleLabelRepository.Setup(repo => repo.RemoveLabelAssignmentFromAccountAsync(1, 10))
                .Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.RevokeExclusiveLabelAsync(1, 10, "AM");

            // Assert
            _mockRoleLabelRepository.Verify(repo => repo.RemoveLabelAssignmentFromAccountAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task RevokeExclusiveLabelAsync_WhenNonExclusiveLabel_ShouldNotCallRepo()
        {
            // Act
            await _roleLabelService.RevokeExclusiveLabelAsync(1, 10, "OTHER");

            // Assert
            _mockRoleLabelRepository.Verify(repo => repo.RemoveLabelAssignmentFromAccountAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
