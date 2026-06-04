// <copyright file="RoleLabelServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Services
{
    public class RoleLabelServiceTests
    {
        private readonly Mock<IRoleLabelRepository> _mockRoleLabelRepository;
        private readonly Mock<ILabelService> _mockLabelService;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<ILogger<RoleLabelService>> _mockLogger;
        private readonly RoleLabelService _roleLabelService;

        public RoleLabelServiceTests()
        {
            _mockRoleLabelRepository = new Mock<IRoleLabelRepository>();
            _mockLabelService = new Mock<ILabelService>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockLogger = new Mock<ILogger<RoleLabelService>>();
            _roleLabelService = new RoleLabelService(_mockRoleLabelRepository.Object, _mockLabelService.Object, _mockRoleRepository.Object, _mockLogger.Object);
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

            _mockRoleLabelRepository.Setup(repo => repo.GetLabelCodeAsync(labelId)).ReturnsAsync("OTHER");
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

            _mockRoleLabelRepository.Setup(repo => repo.GetLabelCodeAsync(labelId)).ReturnsAsync("OTHER");
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

            _mockRoleLabelRepository.Setup(repo => repo.GetLabelCodeAsync(labelId)).ReturnsAsync((string?)null);
            _mockRoleLabelRepository.Setup(repo => repo.DeleteRoleLabelAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act - No exception should be thrown
            await _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId);

            // Assert
            _mockRoleLabelRepository.Verify(repo =>
                repo.DeleteRoleLabelAsync(accountId, contactId, labelId), Times.Once);
        }

        [Theory]
        [InlineData("AM")]
        [InlineData("am")]
        [InlineData("CLP")]
        [InlineData("clp")]
        public async Task DeleteRoleLabelAsync_ExclusiveLabelOnProspect_ThrowsBadRequestException(string labelCode)
        {
            // Arrange
            int accountId = 1;
            int contactId = 2;
            int labelId = 10;

            _mockRoleLabelRepository.Setup(repo => repo.GetLabelCodeAsync(labelId)).ReturnsAsync(labelCode);
            _mockRoleRepository.Setup(repo => repo.IsProspectAccountAsync(accountId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId));

            _mockRoleLabelRepository.Verify(repo => repo.DeleteRoleLabelAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("AM")]
        [InlineData("CLP")]
        public async Task DeleteRoleLabelAsync_ExclusiveLabelOnClientAccount_Succeeds(string labelCode)
        {
            // Arrange
            int accountId = 1;
            int contactId = 2;
            int labelId = 10;

            _mockRoleLabelRepository.Setup(repo => repo.GetLabelCodeAsync(labelId)).ReturnsAsync(labelCode);
            _mockRoleRepository.Setup(repo => repo.IsProspectAccountAsync(accountId)).ReturnsAsync(false);
            _mockRoleLabelRepository.Setup(repo => repo.DeleteRoleLabelAsync(accountId, contactId, labelId)).Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId);

            // Assert
            _mockRoleLabelRepository.Verify(repo => repo.DeleteRoleLabelAsync(accountId, contactId, labelId), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleLabelAsync_NonExclusiveLabelOnProspect_Succeeds()
        {
            // Arrange
            int accountId = 1;
            int contactId = 2;
            int labelId = 10;

            _mockRoleLabelRepository.Setup(repo => repo.GetLabelCodeAsync(labelId)).ReturnsAsync("OTHER");
            _mockRoleLabelRepository.Setup(repo => repo.DeleteRoleLabelAsync(accountId, contactId, labelId)).Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.DeleteRoleLabelAsync(accountId, contactId, labelId);

            // Assert
            _mockRoleLabelRepository.Verify(repo => repo.DeleteRoleLabelAsync(accountId, contactId, labelId), Times.Once);
            _mockRoleRepository.Verify(repo => repo.IsProspectAccountAsync(It.IsAny<int>()), Times.Never);
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

        private void SetupTwoLabels()
        {
            _mockLabelService.Setup(s => s.GetLabelsAsync(It.IsAny<Pagination>()))
                .ReturnsAsync(new Paging<Label>
                {
                    Items = new List<Label>
                    {
                        new Label { LabelId = 1, Code = "CLP", CustomerLabel = "Responsable", CollaboratorLabel = "Maitre dossier", Business = "Transverse", IsVisible = false },
                        new Label { LabelId = 2, Code = "AM", CustomerLabel = "Chargé de mission", CollaboratorLabel = "Resp compte", Business = "Transverse", IsVisible = false },
                    }
                });
        }

        [Fact]
        public async Task AssignRoleLabelFromCodeAsync_WithNullCode_ShouldDoNothing()
        {
            // Act
            await _roleLabelService.AssignRoleLabelFromCodeAsync(null, 1, 1);

            // Assert
            _mockLabelService.Verify(s => s.GetLabelsAsync(It.IsAny<Pagination>()), Times.Never);
            _mockRoleLabelRepository.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleLabelFromCodeAsync_WithClpCode_ShouldRevokeAndAddLabel()
        {
            // Arrange
            SetupTwoLabels();
            _mockRoleLabelRepository.Setup(r => r.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockRoleLabelRepository.Setup(r => r.RemoveLabelAssignmentFromAccountAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockRoleLabelRepository.Setup(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>())).Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.AssignRoleLabelFromCodeAsync("CLP", 5, 9);

            // Assert
            _mockRoleLabelRepository.Verify(r => r.RemoveLabelAssignmentFromAccountAsync(5, 1), Times.Once);
            _mockRoleLabelRepository.Verify(r => r.AddRoleLabelAsync(
                It.Is<RoleLabel>(rl => rl.AccountId == 5 && rl.ContactId == 9 && rl.LabelId == 1)), Times.Once);
        }

        [Fact]
        public async Task AssignRoleLabelFromCodeAsync_WithAmCode_ShouldRevokeAndAddLabel()
        {
            // Arrange
            SetupTwoLabels();
            _mockRoleLabelRepository.Setup(r => r.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockRoleLabelRepository.Setup(r => r.RemoveLabelAssignmentFromAccountAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockRoleLabelRepository.Setup(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>())).Returns(Task.CompletedTask);

            // Act
            await _roleLabelService.AssignRoleLabelFromCodeAsync("AM", 5, 9);

            // Assert
            _mockRoleLabelRepository.Verify(r => r.RemoveLabelAssignmentFromAccountAsync(5, 2), Times.Once);
            _mockRoleLabelRepository.Verify(r => r.AddRoleLabelAsync(
                It.Is<RoleLabel>(rl => rl.AccountId == 5 && rl.ContactId == 9 && rl.LabelId == 2)), Times.Once);
        }

        [Fact]
        public async Task AssignRoleLabelFromCodeAsync_WithUnknownCode_ShouldNotAddLabel()
        {
            // Arrange
            SetupTwoLabels();

            // Act
            await _roleLabelService.AssignRoleLabelFromCodeAsync("UNKNOWN", 5, 9);

            // Assert
            _mockRoleLabelRepository.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
            _mockRoleLabelRepository.Verify(r => r.RemoveLabelAssignmentFromAccountAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleLabelFromCodeAsync_WhenAlreadyAssigned_ShouldNotReassign()
        {
            // Arrange
            SetupTwoLabels();
            _mockRoleLabelRepository.Setup(r => r.HasRoleLabel(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            // Act
            await _roleLabelService.AssignRoleLabelFromCodeAsync("CLP", 5, 9);

            // Assert
            _mockRoleLabelRepository.Verify(r => r.RemoveLabelAssignmentFromAccountAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockRoleLabelRepository.Verify(r => r.AddRoleLabelAsync(It.IsAny<RoleLabel>()), Times.Never);
        }
    }
}
