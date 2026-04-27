// <copyright file="FavoriteServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Services
{
    public class FavoriteServiceTests
    {
        private readonly Mock<IFavoriteRepository> _favoriteRepository;
        private readonly Mock<IRoleEventPublisher> _roleEventPublisher;
        private readonly Mock<ILogger<FavoriteService>> _logger;
        private readonly Fixture _fixture;

        public FavoriteServiceTests()
        {
            _favoriteRepository = new Mock<IFavoriteRepository>(MockBehavior.Strict);
            _roleEventPublisher = new Mock<IRoleEventPublisher>(MockBehavior.Strict);
            _logger = new Mock<ILogger<FavoriteService>>(MockBehavior.Strict);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private FavoriteService CreateService()
        {
            _logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()));

            return new FavoriteService(_favoriteRepository.Object, _roleEventPublisher.Object, _logger.Object);
        }

        [Fact]
        public async Task Should_GetAccountFavoriteList_ReturnsOkResultAsync()
        {
            var listAccountFavoriteMocked = _fixture.Create<List<AccountFavorite>>();
            var role = _fixture.Create<Role>();

            _favoriteRepository.Setup(r => r.GetAccountFavoritesByContactIdAsync(It.IsAny<int>()))
                .ReturnsAsync(listAccountFavoriteMocked);

            _roleEventPublisher.Setup(r => r.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()));

            var service = CreateService();

            var accountsFavorite = await service.GetAccountFavoritesByContactIdAsync(contactId: 1);

            Assert.Equal(listAccountFavoriteMocked, accountsFavorite);
        }

        [Fact]
        public async Task Should_SetAccountFavorite_ReturnsOkResultAsync()
        {
            var role = _fixture.Create<Role>();

            _favoriteRepository.Setup(r => r.UpdateAccountFavoriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            _roleEventPublisher.Setup(r => r.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.SetFavoriteAsync(accountId: 1, contactId: 1, true);

            _favoriteRepository.Verify(x => x.UpdateAccountFavoriteAsync(1, 1, true), Times.Once);
        }

        [Fact]
        public async Task SetAccountFavoriteAsync_Should_Throw_NotFoundException()
        {
            _favoriteRepository.Setup(r => r.UpdateAccountFavoriteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws(new NotFoundException(Errors.NotFoundRoleCode, Errors.NotFoundRoleMessage));

            var service = CreateService();

            var result = async () => await service.SetFavoriteAsync(123, 123, true);

            var exception = await Assert.ThrowsAsync<NotFoundException>(result);
            Assert.Equal(Errors.NotFoundRoleCode, exception.Code);
            Assert.Equal(Errors.NotFoundRoleMessage, exception.Message);
        }

        [Fact]
        public async Task SetFavoriteAsync_Should_Call_PublishRoleCreatedEventAsync()
        {
            var role = _fixture.Build<Role>()
                .With(r => r.AccountId, 1)
                .With(r => r.ContactId, 1)
                .With(r => r.IsFavorite, true)
                .With(r => r.IsSignatory, false)
                .With(r => r.IsDelegation, false)
                .Create();

            _favoriteRepository.Setup(r => r.UpdateAccountFavoriteAsync(1, 1, true))
                .Returns(Task.CompletedTask);

            _roleEventPublisher.Setup(p => p.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

            var service = CreateService();

            await service.SetFavoriteAsync(1, 1, true);

            _roleEventPublisher.Verify(p => p.PublishRoleUpdatedEventAsync(
                It.IsAny<int>(),
                It.IsAny<int>()),
                Times.Once);
        }
    }
}
