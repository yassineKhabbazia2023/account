using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.API.Controllers;

namespace Pulse.Account.API.Tests.Controllers
{
    public class RoleLabelControllerTests
    {
        private readonly Mock<IRoleLabelService> _mockRoleLabelService;
        private readonly RoleLabelController _controller;

        public RoleLabelControllerTests()
        {
            _mockRoleLabelService = new Mock<IRoleLabelService>();
            _controller = new RoleLabelController(_mockRoleLabelService.Object);
        }

        [Fact]
        public async Task CreateRoleLabelAsync_ValidRole_ReturnsOk()
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

            _mockRoleLabelService.Setup(s => s.AddRoleLabelAsync(It.IsAny<RoleLabel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateRoleLabelAsync(roleLabel);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _mockRoleLabelService.Verify(s => s.AddRoleLabelAsync(roleLabel), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleLabelAsync_ValidRequest_ReturnsOk()
        {
            // Arrange
            var deleteRequest = new RoleLabelDeleteRequest
            {
                AccountId = 123,
                ContactId = 456,
                LabelId = 789
            };

            _mockRoleLabelService.Setup(s => s.DeleteRoleLabelAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteRoleLabelAsync(deleteRequest);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _mockRoleLabelService.Verify(s => s.DeleteRoleLabelAsync(
                deleteRequest.AccountId, deleteRequest.ContactId, deleteRequest.LabelId), Times.Once);
        }
    }
}
