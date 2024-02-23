// <copyright file="RolesServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Moq;
using AutoFixture;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Exceptions;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using AccountModel = Pulse.Account.Core.Models.Account;
using FluentAssertions;

namespace Pulse.Account.Core.Tests.Services;

public class RolesServiceTests
{
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [Fact]
    public async Task GetContactRolesAsync_Should_ReturnsOkResultAsync()
    {
        // Arrange
        string accountMocked = File.ReadAllText(@"./MockedResponses/AccountListMocked.json");
        var accountList = JsonSerializer.Deserialize<Paging<AccountModel>>(accountMocked, _jsonOptions) ?? new Paging<AccountModel>();
        var rolesRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        rolesRepository.Setup(repository => repository.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);

        var rolesService = new RolesService(rolesRepository.Object);

        // Act
        var accounts = await rolesService.GetContactRolesAsync(contactId: 123, pageNumber: 0, pageSize: 0);

        // Assert
        Assert.Equal(accountList, accounts);
        rolesRepository.Verify(x => x.GetContactRolesAsync(123, 1, int.MaxValue));
    }

    [Fact]
    public async Task GetSignatoryAsync_Should_Returns_Account_Signatory()
    {
        // Arrange
        var accountId = 116;
        var fixture = new Fixture();
        var expected = new List<Contact> { fixture.Create<Contact>() };

        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(accountId))
            .ReturnsAsync(expected);

        var roleService = new RolesService(roleRepository.Object);

        // Act
        var result = await roleService.GetSignatoryAsync(accountId);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateRole_Should_ReturnsCreatedResultAsync()
    {
        // Arrange
        var roleParam = new CreateRole()
        {
            AccountId = 6,
            ContactId = 6,
            IsFavorite = false,
            IsSignatory = false
        };

        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.CreateRoleAsync(It.IsAny<CreateRole>()))
            .Returns(Task.CompletedTask);
        var roleService = new RolesService(roleRepository.Object);

        // Act
        await roleService.CreateRoleAsync(roleParam);

        // Assert
        roleRepository.VerifyAll();
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldThrow_BadRequestException()
    {
        // Arrange
        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.CreateRoleAsync(null))
            .ThrowsAsync(new BadRequestException(HttpStatusCode.BadRequest.ToString(), Errors.NotNullException));
        var roleService = new RolesService(roleRepository.Object);

        // Act
        Task Roles() => roleService.CreateRoleAsync(null);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(Roles);
    }

    [Fact]
    public async Task UpdateRole_Should_ReturnsOkResultAsync()
    {
        // Arrange
        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.UpdateRoleSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        var roleService = new RolesService(roleRepository.Object);

        // Act
        await roleService.UpdateRoleSignatoryAsync(1, 1, true);

        // Assert
        roleRepository.VerifyAll();
    }
}
