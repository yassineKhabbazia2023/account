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
                        Status = 0
                    },
                    new()
                    {
                        AccountId = 3,
                        Status = 2,
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
                        ContactId = 2,
                        AccountId = 1
                    },
                    new()
                    {
                        ContactId = 2,
                        AccountId = 2
                    },
                    new()
                    {
                        ContactId = 2,
                        AccountId = 3
                    }
                };
                var contacts = new List<ContactEntity>
                {
                    new()
                    {
                        ContactId = 1,
                        Status = "Declared",
                        Email = "email1@abc.com",
                        FirstName = "fname1",
                        LastName = "lname1",
                        Type = "collaborator",
                        PersonaName = "Persona1",
                        Office = "Paris",
                        CreationDate = DateTime.UtcNow
                    },
                    new()
                    {
                        ContactId = 2,
                        Status = "Connected",
                        Email = "email2@abc.com",
                        FirstName = "fname2",
                        LastName = "lname2",
                        Type = "customer",
                        PersonaName = "Persona1",
                        Office = "Paris",
                        CreationDate = DateTime.UtcNow
                    }
                };
                var repository = new StatisticsRepository(context);
                context.DeploymentEntity.AddRange(expected);
                context.RoleEntity.AddRange(roles);
                context.ContactEntity.AddRange(contacts);
                await context.SaveChangesAsync();

                var result = await repository.GetStatisticsAsync(2);

                Assert.NotNull(result);
                Assert.Equal(1, result.AccountToDeploy);
                Assert.Equal(1, result.AccountConnected);
                Assert.Equal(1, result.AccountInProgress);
                Assert.Equal(0, result.ContactInvited);
                Assert.Equal(0, result.ContactDeclared);
                Assert.Equal(1, result.ContactConnected);
            }
        }
    }
}
