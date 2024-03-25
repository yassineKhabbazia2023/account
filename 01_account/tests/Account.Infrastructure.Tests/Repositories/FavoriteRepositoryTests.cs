// <copyright file="FavoriteRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Core.Models;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class FavoriteRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _options;

        public FavoriteRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAccountsFavoriteAsync_Should_ReturnsOkResultAsync()
        {
            using (var context = new AccountContext(_options))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                accountsModel.ForEach(account => account.RoleEntity.First().IsFavorite = true);
                context.AccountEntity.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var contactId = accountsModel.Select(account => account.RoleEntity.Where(role => role.IsFavorite == true).Select(role => role.ContactId).FirstOrDefault()).FirstOrDefault();
                var favoriteRepository = new FavoriteRepository(context);

                var expectedAccount = accountsModel.Select(entity => new AccountFavorite()
                {
                    AccountId = entity.AccountId,
                    LegalName = entity.LegalName,
                    IconName = entity.IconName
                });

                // Act
                var accounts = await favoriteRepository.GetAccountFavoritesByContactIdAsync(contactId);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(expectedAccount);
                var accountReceived = JsonConvert.SerializeObject(accounts.FirstOrDefault());
                Assert.Contains(accountReceived, accountExpect);
            }
        }

        [Fact]
        public async Task UpdateAccountFavoriteAsync_Should_ReturnsOkResultAsync()
        {
            using (var context = new AccountContext(_options))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                accountsModel.ForEach(account => account.RoleEntity.First().IsFavorite = true);
                context.AccountEntity.AddRange(accountsModel);
                await context.SaveChangesAsync();

                var role = accountsModel.Select(account => account.RoleEntity.Where(role => role.IsFavorite == true).Select(role => role).First()).First();
                var favoriteRepository = new FavoriteRepository(context);

                // Act
                await favoriteRepository.UpdateAccountFavoriteAsync(role.AccountId, role.ContactId, false);
                var accountFavorite = await favoriteRepository.GetAccountFavoritesByContactIdAsync(role.ContactId);

                // Assert
                Assert.Empty(accountFavorite);
            }
        }

        [Fact]
        public async Task UpdateAccountFavoriteAsync_Should_Throw_NotFoundException()
        {
            using (var context = new AccountContext(_options))
            {
                // Arrange
                var accountsMock = _fixture.Create<List<AccountEntity>>();

                context.AccountEntity.AddRange(accountsMock);
                context.SaveChanges();

                var favoriteRepository = new FavoriteRepository(context);

                // Act
                Task UpdateFavorite() => favoriteRepository.UpdateAccountFavoriteAsync(123, 123, false);

                // Assert
                await Assert.ThrowsAsync<NotFoundException>(UpdateFavorite);
            }
        }
    }
}
