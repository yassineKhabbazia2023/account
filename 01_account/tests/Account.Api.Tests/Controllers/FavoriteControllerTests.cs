// <copyright file="FavoriteControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Pulse.Account.API;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Account.Api.Tests.Controllers
{
    public class FavoriteControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly Mock<IFavoriteService> _favoriteService;

        public FavoriteControllerTests()
        {
            _favoriteService = new Mock<IFavoriteService>(MockBehavior.Strict);
        }

        [Fact]
        public async Task GetAccountFavorites_Should_ReturnsOkResultAsync()
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

            _favoriteService.Setup(service => service.GetAccountFavoritesByContactIdAsync(It.IsAny<int>())).ReturnsAsync(accountJson);

            var favoriteController = new FavoriteController(_favoriteService.Object);

            // Act
            var accounts = await favoriteController.GetAccountFavoritesByContactIdAsync(contactId: 1);
            var resultAccounts = accounts?.Result as OkObjectResult;

            // Assert
            Assert.Equal(accountJson, resultAccounts?.Value);
        }

        [Fact]
        public async Task SetFavorite_Should_ReturnsOkResultAsync()
        {
            // Arrange
            _favoriteService.Setup(service => service.SetFavoriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            var favoriteController = new FavoriteController(_favoriteService.Object);

            // Act
            var result = await favoriteController.SetFavoriteAsync(accountId: 5, contactId: 1, true);
            var resultAccounts = result as OkResult;

            // Assert
            Assert.Equal(200, resultAccounts?.StatusCode);
        }

        [Fact]
        public async Task SetFavorite_Should_Throw_NotFoundException()
        {
            // Arrange
            _favoriteService
                .Setup(service => service.SetFavoriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws(new NotFoundException(Errors.NotFoundRoleContactCode, Errors.NotFoundRoleContactMessage));

            var favoriteController = new FavoriteController(_favoriteService.Object);

            // Act
            var result = async () => await favoriteController.SetFavoriteAsync(accountId: 5, contactId: 1, true);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundRoleContactCode, exception.Code);
            Assert.Equal(Errors.NotFoundRoleContactMessage, exception.Message);
        }
    }
}
