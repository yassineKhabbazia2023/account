// <copyright file="StatisticsRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
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
            context.ChangeTracker.Clear();

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

    [Fact]
    public async Task GetAccountPercentageCustomerRelationAsync_Should_ReturnsCorrectPercentage()
    {
        await using var context = new AccountContext(_options);
        var accountsEntity = _fixture.Build<AccountEntity>()
            .Without(a => a.RoleEntity)
            .CreateMany(100).ToList();

        var accountFirstId = accountsEntity.First().AccountId;
        var contactId = 1;

        var contactEntity = _fixture.Build<ContactEntity>()
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .With(c => c.ContactId, contactId)
            .Without(c => c.RoleEntity)
            .Create();

        var roleEntity = _fixture.Build<RoleEntity>()
            .With(r => r.ContactId, contactId)
            .With(r => r.AccountId, accountFirstId)
            .With(r => r.IsCustomerRelation, true)
            .With(r => r.Contact, contactEntity)
            .Without(r => r.Account)
            .Create();

        var repository = new StatisticsRepository(context);
        context.RoleEntity.AddRange(roleEntity);
        context.AccountEntity.AddRange(accountsEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Hack to fix the test quickly
        var accountsCount = context.AccountEntity.Count();
        var rolesCount = context.RoleEntity.Count(re => re.Contact.Type.Equals(nameof(ContactType.Collaborator), StringComparison.OrdinalIgnoreCase));
        var realExpected = (double)rolesCount / accountsCount * 100;

        var percentage = await repository.GetAccountPercentageCustomerRelationAsync();

        realExpected.Should().BeApproximately(percentage, 0.01);
    }

    [Fact]
    public async Task GetAccountsPerClientCountAsync_ShouldReturnCorrectCounts()
    {
        using var context = new AccountContext(_options);

        var contacts = new List<ContactEntity>
        {
            new()
            {
                ContactId = 1,
                Email = "mail1",
                Type = "Customer",
                FirstName = "fname1",
                LastName = "lname1",
                PersonaName = "dirigeant",
                IsActive = true,
            },
            new()
            {
                ContactId = 2,
                Email = "mail2",
                Type = "Customer",
                FirstName = "fname2",
                LastName = "lname2",
                PersonaName = "dirigeant",
                IsActive = true,
            },
            new()
            {
                ContactId = 3,
                Email = "mail3",
                Type = "Collaborator",
                FirstName = "fname3",
                LastName = "lname3",
                PersonaName = "dirigeant",
                IsActive = true,
            }
        };
        context.ContactEntity.AddRange(contacts);

        var accounts = new List<AccountEntity>
        {
            new()
            {
                AccountId = 100,
                AccountNumber = "num100",
                LegalName = "legal100",
                CreatedBy = "moi",
                IsActive = true,
            },
            new()
            {
                AccountId = 200,
                AccountNumber = "num200",
                LegalName = "legal200",
                CreatedBy = "moi",
                IsActive = true,
            }
        };
        context.AccountEntity.AddRange(accounts);

        context.RoleEntity.AddRange(
            new RoleEntity { ContactId = 1, AccountId = 100 },
            new RoleEntity { ContactId = 1, AccountId = 200 },
            new RoleEntity { ContactId = 2, AccountId = 100 },
            new RoleEntity { ContactId = 3, AccountId = 200 });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new StatisticsRepository(context);

        // Act
        var result = await repository.GetAccountsPerClientCountAsync();

        // Assert
        result.Should().ContainInOrder(
            (1, 1),
            (2, 1));
    }

    [Fact]
    public async Task GetClientsPerAccountCountAsync_ShouldReturnCorrectCounts()
    {
        using var context = new AccountContext(_options);

        var contacts = new List<ContactEntity>
        {
            new()
            {
                ContactId = 1,
                Email = "mail1",
                Type = "Customer",
                FirstName = "fname1",
                LastName = "lname1",
                PersonaName = "dirigeant",
                IsActive = true,
            },
            new()
            {
                ContactId = 2,
                Email = "mail2",
                Type = "Customer",
                FirstName = "fname2",
                LastName = "lname2",
                PersonaName = "dirigeant",
                IsActive = true,
            },
            new()
            {
                ContactId = 3,
                Email = "mail3",
                Type = "Collaborator",
                FirstName = "fname3",
                LastName = "lname3",
                PersonaName = "dirigeant",
                IsActive = true,
            }
        };
        context.ContactEntity.AddRange(contacts);

        var accounts = new List<AccountEntity>
        {
            new()
            {
                AccountId = 100,
                AccountNumber = "num100",
                LegalName = "legal100",
                CreatedBy = "moi",
                IsActive = true,
            },
            new()
            {
                AccountId = 200,
                AccountNumber = "num200",
                LegalName = "legal200",
                CreatedBy = "moi",
                IsActive = true,
            }
        };
        context.AccountEntity.AddRange(accounts);

        context.RoleEntity.AddRange(
            new RoleEntity { ContactId = 1, AccountId = 100 },
            new RoleEntity { ContactId = 1, AccountId = 200 },
            new RoleEntity { ContactId = 2, AccountId = 100 },
            new RoleEntity { ContactId = 3, AccountId = 200 });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new StatisticsRepository(context);

        // Act
        var result = await repository.GetClientsPerAccountCountAsync();

        // Assert
        result.Should().ContainInOrder(
            (1, 1),
            (2, 1));
    }

    [Fact]
    public async Task GetEntityCountByTypeAsync_ShouldReturnCorrectCounts()
    {
        using var context = new AccountContext(_options);

        var contactId = 10;

        var accounts = new List<AccountEntity>
        {
            new() { AccountId = 1, AccountNumber = "num1", LegalName = "Client 1", CreatedBy = "test", IsActive = true, AccountType = AccountType.CLIENT.ToString() },
            new() { AccountId = 2, AccountNumber = "num2", LegalName = "Client 2", CreatedBy = "test", IsActive = true, AccountType = AccountType.CLIENT.ToString() },
            new() { AccountId = 3, AccountNumber = "num3", LegalName = "Prospect 1", CreatedBy = "test", IsActive = true, AccountType = AccountType.PROSPECT.ToString() },
            new() { AccountId = 4, AccountNumber = "num4", LegalName = "Other", CreatedBy = "test", IsActive = true, AccountType = "OTHER" },
            new() { AccountId = 5, AccountNumber = "num5", LegalName = "Client sans role", CreatedBy = "test", IsActive = true, AccountType = AccountType.CLIENT.ToString() },
        };

        var roles = new List<RoleEntity>
        {
            new() { ContactId = contactId, AccountId = 1 },
            new() { ContactId = contactId, AccountId = 2 },
            new() { ContactId = contactId, AccountId = 3 },
            new() { ContactId = contactId, AccountId = 4 },
            new() { ContactId = 999, AccountId = 5 },
        };

        context.AccountEntity.AddRange(accounts);
        context.RoleEntity.AddRange(roles);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new StatisticsRepository(context);

        // Act
        var result = await repository.GetEntityCountByTypeAsync(contactId);

        // Assert
        result.RegularEntitiesCount.Should().Be(2);
        result.ProspectEntitiesCount.Should().Be(1);
    }

    [Fact]
    public async Task GetEntityCountByTypeAsync_EmptyDatabase_ShouldReturnZeroCounts()
    {
        using var context = new AccountContext(_options);
        var repository = new StatisticsRepository(context);

        // Act
        var result = await repository.GetEntityCountByTypeAsync(1);

        // Assert
        result.RegularEntitiesCount.Should().Be(0);
        result.ProspectEntitiesCount.Should().Be(0);
    }
}
