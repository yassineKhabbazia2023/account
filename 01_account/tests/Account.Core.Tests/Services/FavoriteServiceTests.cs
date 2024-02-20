// <copyright file="FavoriteServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class FavoriteServiceTests
    {
        private Mock<IFavoriteRepository> _favoriteRepository;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public FavoriteServiceTests()
        {
            _favoriteRepository = new Mock<IFavoriteRepository>(MockBehavior.Strict);
        }

        [Fact]
        public async Task Should_GetAccountFavoriteList_ReturnsOkResultAsync()
        {
            string accountMocked = File.ReadAllText(@"./MockedResponses/AccountFavoriteMocked.json");
            var accountFavoriteList = JsonSerializer.Deserialize<List<AccountFavorite>>(accountMocked, _jsonOptions) ?? new List<AccountFavorite>();
            _favoriteRepository.Setup(repository => repository.GetAccountFavoritesByContactIdAsync(It.IsAny<int>())).ReturnsAsync(accountFavoriteList);

            var accountService = new FavoriteService(_favoriteRepository.Object);

            // Act
            var accountsFavorite = await accountService.GetAccountFavoritesByContactIdAsync(contactId: 1);

            // Assert
            Assert.Equal(accountFavoriteList, accountsFavorite);
        }

        [Fact]
        public async Task Should_SetAccountFavorite_ReturnsOkResultAsync()
        {
            _favoriteRepository.Setup(repository => repository.UpdateAccountFavoriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            var accountService = new FavoriteService(_favoriteRepository.Object);

            // Act
            await accountService.SetFavoriteAsync(accountId: 1, contactId: 1, true);

            // Assert
            _favoriteRepository.Verify(x => x.UpdateAccountFavoriteAsync(1, 1, true), Times.Once);
        }
    }
}
