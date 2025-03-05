// <copyright file="StatisticsRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

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
                    Status = 2
                },
                new()
                {
                    AccountId = 2,
                    Status = 1
                },
                new()
                {
                    AccountId = 3,
                    Status = 3,
                },
                new()
                {
                    AccountId = 4,
                    Status = 4,
                }
            };

            var roles = new List<RoleEntity>
            {
                new()
                {
                    ContactId = 1,
                    AccountId = 6,
                    Account = new AccountEntity()
                    {
                        AccountId = 6,
                        IsActive = true,
                        AccountNumber = "123",
                        CreatedBy = "test@test.fr",
                        LegalName = "Account 6",
                    },
                },
                new()
                {
                    ContactId = 2,
                    AccountId = 1,
                    Account = new AccountEntity()
                    {
                        AccountId = 1,
                        IsActive = true,
                        AccountNumber = "456",
                        CreatedBy = "test@test.fr",
                        LegalName = "Account 1"
                    },
                },
                new()
                {
                    ContactId = 2,
                    AccountId = 2,
                    Account = new AccountEntity()
                    {
                        AccountId = 2,
                        IsActive = true,
                        AccountNumber = "789",
                        CreatedBy = "test@test.fr",
                        LegalName = "Account 2"
                    },
                },
                new()
                {
                    ContactId = 2,
                    AccountId = 3,
                    Account = new AccountEntity()
                    {
                        AccountId = 3,
                        IsActive = true,
                        AccountNumber = "910",
                        CreatedBy = "test@test.fr",
                        LegalName = "Account 3"
                    },
                },
                new()
                {
                    ContactId = 2,
                    AccountId = 4,
                    Account = new AccountEntity()
                    {
                        AccountId = 4,
                        IsActive = true,
                        AccountNumber = "112",
                        CreatedBy = "test@test.fr",
                        LegalName = "Account 4"
                    },
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
                    Type = "Collaborator",
                    PersonaName = "Persona1",
                    Office = "Paris",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    ContactId = 2,
                    Status = "Connected",
                    Email = "email2@abc.com",
                    FirstName = "fname2",
                    LastName = "lname2",
                    Type = "Customer",
                    PersonaName = "Persona1",
                    Office = "Paris",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
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
            Assert.Equal(1, result.AccountRevoked);
            Assert.Equal(0, result.ContactInvited);
            Assert.Equal(0, result.ContactDeclared);
            Assert.Equal(1, result.ContactConnected);
        }
    }
}
