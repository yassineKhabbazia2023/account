// <copyright file="RolesControllerTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using AutoMapper.Configuration.Annotations;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
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
            var accounts = await rolesController.GetContactRolesAsync(contactId: 123, page: 1, limit: 4);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountList, resultAccounts?.Value);
        }

        [Fact]
        public async Task GetSignatory_Should_ReturnOkResultAsync()
        {
            // Arrange
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            var expected = new List<Signatory>()
                {
                    new Signatory()
                    {
                        ContactId = 6,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        ContactEmail = "Email",
                    }
                };
            rolesService.Setup(service => service.GetSignatoryAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);
            var rolesController = new RolesController(rolesService.Object);

            // Act
            var result = await rolesController.GetSignatoryAsync(6);

            // Assert
            Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        }

        [Fact]
        public async Task CreateRole_Should_ReturnCreatedResultAsync()
        {
            // Arrange
            var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
            var roleParam = new Role()
            {
                RoleId = 6,
                AccountId = 6,
                ContactId = 6,
                IsFavorite = false,
                IsSignatory = false
            };
            rolesService.Setup(service => service.CreateRoleAsync(It.IsAny<Role>()))
                .ReturnsAsync(1);
            var rolesController = new RolesController(rolesService.Object);

            // Act
            var actionResult = await rolesController.CreateRoleAsync(roleParam);
            var result = actionResult as ObjectResult;

            // Assert
            result.StatusCode.Should().Be(201);
            rolesService.Verify(x => x.CreateRoleAsync(roleParam), Times.Once);
        }
    }
}
