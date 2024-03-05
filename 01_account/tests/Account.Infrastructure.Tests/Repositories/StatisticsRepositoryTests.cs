using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class StatisticsRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _options;

        public StatisticsRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetStatisticsAsync_DefaultParam_ReturnExpected()
        {
            using (var context = new AccountContext(_options))
            {
                var expected = new List<DeploymentEntity>
                {
                    new()
                    {
                        AccountId = 1,
                        Status = 1
                    },
                    new()
                    {
                        AccountId = 2,
                        Status = 1
                    },
                    new()
                    {
                        AccountId = 1,
                        Status = 2
                    },
                    new()
                    {
                        AccountId = 3,
                        Status = 0,
                    }
                };
                var roles = new List<RoleEntity>
                {
                    new()
                    {
                        ContactId = 1,
                        AccountId = 1
                    },
                    new()
                    {
                        ContactId = 1,
                        AccountId = 2
                    },
                    new()
                    {
                        ContactId = 2,
                        AccountId = 3
                    }
                };
                var repository = new StatisticsRepository(context);
                context.DeploymentEntity.AddRange(expected);
                context.RoleEntity.AddRange(roles);
                await context.SaveChangesAsync();

                var result = await repository.GetStatisticsAsync(1);

                Assert.NotNull(result);
                Assert.Equal(0, result.AccountToDeploy);
                Assert.Equal(1, result.AccountConnected);
                Assert.Equal(2, result.AccountInProgress);
            }
        }
    }
}
