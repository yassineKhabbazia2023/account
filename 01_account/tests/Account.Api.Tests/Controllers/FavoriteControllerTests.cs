// <copyright file="FavoriteControllerTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Pulse.Account.API;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;

namespace Account.Api.Tests.Controllers
{
    public class FavoriteControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly Mock<IAccountService> _accountService;

        public FavoriteControllerTests()
        {
            _accountService = new Mock<IAccountService>(MockBehavior.Strict);
        }

        [Fact]
        public async Task Should_GetAccountFavoriteList_ReturnsOkResultAsync()
        {
            // Arrange
            var accountJson = new List<AccountFavorite>()
            {
                new()
                {
                    AccountId = 5,
                    LegalName = "JEAN LEVAGE",
                    IconName = "jeanlevage"
                }
            };

            _accountService.Setup(service => service.GetAccountFavoritesAsync(It.IsAny<int>())).ReturnsAsync(accountJson);

            var favoriteController = new FavoriteController(_accountService.Object);

            // Act
            var accounts = await favoriteController.GetAccountFavoritesAsync(contactId: 1);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountJson, resultAccounts?.Value);
        }
    }
}
