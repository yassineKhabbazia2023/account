// <copyright file="FavoriteServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Identity.Client;
using Moq;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class FavoriteServiceTests
    {
        private Mock<IFavoriteRepository> _favoriteRepository;
        private readonly Fixture _fixture;

        public FavoriteServiceTests()
        {
            _favoriteRepository = new Mock<IFavoriteRepository>(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task Should_GetAccountFavoriteList_ReturnsOkResultAsync()
        {
            var listAccountFavoriteMocked = _fixture.Create<List<AccountFavorite>>();
            _favoriteRepository.Setup(repository => repository.GetAccountFavoritesByContactIdAsync(It.IsAny<int>())).ReturnsAsync(listAccountFavoriteMocked);

            var accountService = new FavoriteService(_favoriteRepository.Object);

            // Act
            var accountsFavorite = await accountService.GetAccountFavoritesByContactIdAsync(contactId: 1);

            // Assert
            Assert.Equal(listAccountFavoriteMocked, accountsFavorite);
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

        [Fact]
        public async Task SetAccountFavoriteAsync_Should_Throw_NotFoundException()
        {
            _favoriteRepository
                .Setup(repository => repository.UpdateAccountFavoriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws(new NotFoundException(Errors.NotFoundRoleCode, Errors.NotFoundRoleMessage));

            var favoriteService = new FavoriteService(_favoriteRepository.Object);

            // Act
            var result = async () => await favoriteService.SetFavoriteAsync(123, 123, true);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundRoleCode, exception.Code);
            Assert.Equal(Errors.NotFoundRoleMessage, exception.Message);
        }
    }
}
