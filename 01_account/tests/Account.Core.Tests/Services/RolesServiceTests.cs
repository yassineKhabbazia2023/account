// <copyright file="RolesServiceTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json;
using Moq;
using Pulse.Account.Core.Interfaces;
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
        var rolesRepository = new Mock<IRolesRepository>(MockBehavior.Strict);
        rolesRepository.Setup(repository => repository.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);

        var rolesService = new RolesService(rolesRepository.Object);

        // Act
        var accounts = await rolesService.GetContactRolesAsync(contactId: 123, page: 1, limit: 4);

        // Assert
        Assert.Equal(accountList, accounts);
    }
}
