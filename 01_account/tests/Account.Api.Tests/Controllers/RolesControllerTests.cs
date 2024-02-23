// <copyright file="RolesControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Account.Api.Tests.Controllers
{
    public class RolesControllerTests
    {
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        [Fact]
        public async Task GetContactRoles_Should_ReturnsOkResultAsync()
        {
            // Arrange
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
            var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            rolesService.Setup(service => service.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);

            var rolesController = new RolesController(rolesService.Object);

            // Act
            var accounts = await rolesController.GetContactRolesAsync(contactId: 123, pageNumber: 1, pageSize: 4);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountList, resultAccounts?.Value);
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
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            var roleParam = new CreateRole()
            {
                AccountId = 6,
                ContactId = 6,
                IsFavorite = false,
                IsSignatory = false
            };
            rolesService.Setup(service => service.CreateRoleAsync(It.IsAny<CreateRole>()))
                .Returns(Task.CompletedTask);
            var rolesController = new RolesController(rolesService.Object);

            // Act
            var actionResult = await rolesController.CreateRoleAsync(roleParam);
            var result = actionResult as StatusCodeResult;

            // Assert
            result.StatusCode.Should().Be(201);
            rolesService.Verify(x => x.CreateRoleAsync(roleParam), Times.Once);
        }

        [Fact]
        public async Task CreateRole_Should_ReturnBadRequestExceptionAsync()
        {
            // Arrange
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            rolesService.Setup(service => service.CreateRoleAsync(It.IsAny<CreateRole>()))
                .Throws(new BadRequestException(HttpStatusCode.BadRequest.ToString(), Errors.NotNullException));
            var rolesController = new RolesController(rolesService.Object);

            // Act
            Task Roles() => rolesController.CreateRoleAsync(null);

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
    }
}
