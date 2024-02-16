// <copyright file="RolesServiceTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Services;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Kpmg.Account.Core.Tests.Services;

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
        var accounts = await rolesService.GetContactRolesAsync(contactId: 123, page: 0, limit: 0);

        // Assert
        Assert.Equal(accountList, accounts);
        rolesRepository.Verify(x => x.GetContactRolesAsync(123, 1, int.MaxValue));
    }

    [Fact]
    public async Task GetSignatory_Should_ReturnsOkResultAsync()
    {
        // Arrange
        var signatoryContactId = 6;
        var expected = new List<Signatory>()
                {
                    new Signatory()
                    {
                        ContactId = signatoryContactId,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        ContactEmail = "Email",
                    }
                };
        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .ReturnsAsync(expected);
        var roleService = new RolesService(roleRepository.Object);

        // Act
        var result = await roleService.GetSignatoryAsync(signatoryContactId);

        // Assert
        Assert.Equal(expected, result);
    }
}
