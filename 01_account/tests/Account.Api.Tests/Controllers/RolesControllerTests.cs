// <copyright file="RolesControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Account.Api.Tests.Controllers
{
    public class RolesControllerTests
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private Fixture _fixture;

        public RolesControllerTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetContactRoles_Should_ReturnsOkResultAsync()
        {
            // Arrange
            var accountMocked = _fixture.Create<Paging<Pulse.Account.Core.Models.Account>>();
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            rolesService.Setup(service => service.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountMocked);

            var rolesController = new RolesController(rolesService.Object);

            // Act
            var accounts = await rolesController.GetContactRolesAsync(contactId: 123, pageNumber: 1, pageSize: 4);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountMocked, resultAccounts?.Value);
        }

        [Fact]
        public async Task GetSignatoryAsync_Should_Returns_Account_Signatory()
        {
            // Arrange
            var accountId = 116;
            var fixture = new Fixture();
            var expected = new List<Contact> { fixture.Create<Contact>() };

            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            rolesService.Setup(service => service.GetSignatoryAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);
            var rolesController = new RolesController(rolesService.Object);

            // Act
            var result = await rolesController.GetSignatoryAsync(accountId);

            // Assert
            Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        }

        [Fact]
        public async Task CreateRole_Should_ReturnCreatedResultAsync()
        {
            // Arrange
            var mockRoleService = new Mock<IRolesService>(MockBehavior.Strict);
            var roleRequest = new CreateRoleRequest
            {
                AccountId = 6,
                ContactId = 6,
                IsFavorite = false,
                IsSignatory = false
            };

            mockRoleService.Setup(service => service.CreateRoleAsync(It.IsAny<CreateRoleRequest>()))
                .Returns(Task.CompletedTask);

            var rolesController = new RolesController(mockRoleService.Object);

            // Act
            var result = await rolesController.CreateRoleAsync(roleRequest);

            // Assert
            mockRoleService.Verify(s => s.CreateRoleAsync(roleRequest), Times.Once);
            Assert.IsType<CreatedResult>(result);
        }

        [Fact]
        public async Task CreateRole_Should_ReturnBadRequestExceptionAsync()
        {
            // Arrange
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            rolesService.Setup(service => service.CreateRoleAsync(It.IsAny<CreateRoleRequest>()))
                .Throws(new BadRequestException(Errors.BadRequestRoleCode, Errors.BadRequestRoleMessage));
            var rolesController = new RolesController(rolesService.Object);

            // Act
            Task Roles() => rolesController.CreateRoleAsync(null!);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(Roles);
        }

        [Fact]
        public async Task UpdateRole_Should_ReturnOkResultAsync()
        {
            // Arrange
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            rolesService.Setup(service => service.UpdateRoleSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            var rolesController = new RolesController(rolesService.Object);

            // Act
            var actionResult = await rolesController.UpdateRoleSignatoryAsync(1, 1, true);
            var result = actionResult as StatusCodeResult;

            // Assert
            result.StatusCode.Should().Be(200);
            rolesService.Verify(x => x.UpdateRoleSignatoryAsync(1, 1, true), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldDeleteRole()
        {
            // Arrange
            var roleService = new Mock<IRolesService>();
            roleService.Setup(service => service.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            var roleController = new RolesController(roleService.Object);

            // Act
            var result = await roleController.DeleteRoleAsync(1, 1) as StatusCodeResult;

            // Assert
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldThrowNotFoundException()
        {
            // Arrange
            var roleService = new Mock<IRolesService>();
            roleService.Setup(service => service.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new NotFoundException(Errors.NotFoundRoleCode, Errors.NotFoundRoleMessage));
            var roleController = new RolesController(roleService.Object);

            // Act
            Task DeleteRole() => roleController!.DeleteRoleAsync(1, 1);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(DeleteRole);
        }

        [Fact]
        public async Task DeleteRoleAsync_ShouldThrowBadRequestException()
        {
            // Arrange
            var roleService = new Mock<IRolesService>();
            roleService.Setup(service => service.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new BadRequestException(Errors.CannotDeleteSignatoryCode, Errors.CannotDeleteSignatoryMessage));
            var roleController = new RolesController(roleService.Object);

            // Act
            Task DeleteRole() => roleController!.DeleteRoleAsync(1, 1);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
        }
    }
}
