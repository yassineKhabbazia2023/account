// <copyright file="RolesControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using AutoMapper.Configuration.Annotations;
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
    }
}
