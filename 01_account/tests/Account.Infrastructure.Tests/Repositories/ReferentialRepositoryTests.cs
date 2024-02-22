// <copyright file="ReferentialRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class ReferentialRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _options;

        public ReferentialRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetHubsAsync_Should_Return_HubList()
        {
            using (var context = new AccountContext(_options))
            {
                var tHubs = _fixture.CreateMany<THub>();
                context.AddRange(tHubs);
                await context.SaveChangesAsync();

                var repository = new ReferentialRepository(context);

                var result = await repository.GetHubsAsync();

                Assert.NotNull(result);
                Assert.Equal(tHubs.Count(), result.Count());
            }
        }
    }
}
