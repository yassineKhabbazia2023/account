// <copyright file="DelegationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DelegationRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public DelegationRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegationAndRole()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<ContactEntity>();
            var tDelegatee = _fixture.Create<ContactEntity>();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
                DelegatorId = tDelegator.ContactId,
                DelegationDetails = new List<DelegationDetails>
                {
                    new()
                    {
                        DelegateeId = tDelegatee.ContactId,
                        StartDate = DateTime.UtcNow,
                        Status = "enabled",
                        IsRoleToCreate = true
                    },
                },
                AccountIds = new List<int> { tAccount.AccountId }
            };
            var roles = new List<CreateRoleRequest>
            {
                new()
                {
                    AccountId = tAccount.AccountId,
                    ContactId = tDelegatee.ContactId,
                    IsFavorite = false,
                    IsSignatory = false,
                    IsDelegation = true,
                }
            };

            await repository.CreateDelegationAsync(createDelegation, roles);

            var createdDelegation = await context
                .DelegationEntity
                .FirstOrDefaultAsync(d => d.DelegatorId == tDelegator.ContactId
                && d.DelegateeId == tDelegatee.ContactId
                && d.Account.FirstOrDefault(a => a.AccountId == tAccount.AccountId) != null);

            Assert.NotNull(createdDelegation);

            var createdRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == tAccount.AccountId && r.ContactId == tDelegatee.ContactId);
            Assert.NotNull(createdRole);
            Assert.True(createdRole.IsDelegation);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenDelegatorDoesNotExist_ShouldThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegatee = _fixture.Create<ContactEntity>();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
                DelegatorId = 0,
                DelegationDetails = new List<DelegationDetails>
                {
                    new()
                    {
                        DelegateeId = tDelegatee.ContactId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(5),
                        Status = "pending",
                    }
                },
                AccountIds = new List<int> { tAccount.AccountId }
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation, null!));
            Assert.Equal(Errors.NotFoundContactsCode, result.Code);
            Assert.Equal(Errors.NotFoundContactsMessage, result.Message);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenDelegateeNotExists_ShouldThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<ContactEntity>();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
                DelegatorId = tDelegator.ContactId,
                DelegationDetails = new List<DelegationDetails>
                {
                    new()
                    {
                        DelegateeId = 0,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(5),
                        Status = "pending",
                    }
                },
                AccountIds = new List<int> { tAccount.AccountId },
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation, null!));
            Assert.Equal(Errors.NotFoundContactsCode, result.Code);
            Assert.Equal(Errors.NotFoundContactsMessage, result.Message);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenAccountNotExists_ShouldThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Contacts
            var tDelegator = _fixture.Create<ContactEntity>();
            var tDelegatee = _fixture.Create<ContactEntity>();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
                DelegatorId = tDelegator.ContactId,
                DelegationDetails = new List<DelegationDetails>
                {
                    new()
                    {
                        DelegateeId = tDelegatee.ContactId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(5),
                        Status = "pending",
                    }
                },
                AccountIds = new List<int> { 3 }
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation, null!));
            Assert.Equal(Errors.NotFoundAccountsCode, result.Code);
            Assert.Equal(Errors.NotFoundAccountsMessage, result.Message);
        }
    }

    [Fact]
    public async Task GetContactDelegationsAsync_WhenContactIdIsValid_ShouldReturnContactDelegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            await context.AccountEntity.AddAsync(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<ContactEntity>();
            var tDelegatee = _fixture.Create<ContactEntity>();
            await context.ContactEntity.AddRangeAsync(new List<ContactEntity> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new DelegationEntity
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "pending",
                Note = "Note",
                Account = new List<AccountEntity>
                {
                    tAccount
                }
            };
            await context.DelegationEntity.AddAsync(tDelegation);
            await context.SaveChangesAsync();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var contactDelegations = await repository.GetContactDelegationsAsync(tDelegatee.ContactId);

            Assert.Equal(1, contactDelegations.Count);
            var contactDelegation = contactDelegations.FirstOrDefault();
            Assert.NotNull(contactDelegation);
            Assert.Equal(contactDelegation.StartDate, tDelegation.StartDate);
            Assert.Equal(contactDelegation.EndDate, tDelegation.EndDate);
            Assert.Equal(contactDelegation.Status, tDelegation.Status);
            Assert.Equal(contactDelegation.Note, tDelegation.Note);
            Assert.Equal(contactDelegation.CreationDate, tDelegation.CreationDate);
            Assert.Equal(contactDelegation.Accounts!.Count(), tDelegation.Account.Count);
            Assert.Equal(contactDelegation.Delegator!.ContactId, tDelegation.DelegatorId);
        }
    }

    [Fact]
    public async Task GetDelegationsAsync_WhenRequestIsValid_ShouldReturnDelegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            await context.AccountEntity.AddAsync(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<ContactEntity>();
            var tDelegatee = _fixture.Create<ContactEntity>();
            var anotherDelegatee = _fixture.Create<ContactEntity>();
            await context.ContactEntity.AddRangeAsync(new List<ContactEntity> { tDelegator, tDelegatee, anotherDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new DelegationEntity()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "pending",
                Note = "Note",
                Account = new List<AccountEntity>
                {
                    tAccount
                }
            };

            var anothetDelegationEntity = new DelegationEntity()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(15),
                DelegatorId = tDelegator.ContactId,
                DelegateeId = anotherDelegatee.ContactId,
                Status = "pending",
                Note = "Note 2",
                Account = new List<AccountEntity>
                {
                    tAccount
                }
            };
            await context.DelegationEntity.AddRangeAsync(new List<DelegationEntity> { tDelegation, anothetDelegationEntity });
            await context.SaveChangesAsync();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var delegationList = await repository.GetDelegationsAsync(tDelegator.ContactId, tDelegatee.ContactId);

            Assert.Equal(1, delegationList.Count);
            var delegation = delegationList.FirstOrDefault();
            Assert.NotNull(delegation);
            Assert.Equal(delegation.StartDate, tDelegation.StartDate);
            Assert.Equal(delegation.EndDate, tDelegation.EndDate);
            Assert.Equal(delegation.Status, tDelegation.Status);
            Assert.Equal(delegation.Note, tDelegation.Note);
            Assert.Equal(delegation.CreationDate, tDelegation.CreationDate);
            Assert.Equal(delegation.Accounts!.Count(), tDelegation.Account.Count);
            Assert.Equal(delegation.Delegator!.ContactId, tDelegation.DelegatorId);
        }
    }

    [Fact]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsValidAndStartDateInFuture_ShouldDeleteDelegationButNotRole()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var account = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(account);
            await context.SaveChangesAsync();

            var contact = _fixture.Create<ContactEntity>();
            context.ContactEntity.Add(contact);
            await context.SaveChangesAsync();

            var delegation = new DelegationEntity
            {
                Delegatee = contact,
                StartDate = DateTime.MaxValue,
                Status = "enabled",
                Account = new List<AccountEntity> { account }
            };

            context.DelegationEntity.Add(delegation);
            await context.SaveChangesAsync();

            var role = new RoleEntity
            {
                Account = account,
                Contact = contact,
                IsDelegation = true,
            };
            context.RoleEntity.Add(role);
            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            await repository.DeleteDelegationAsync(delegation.DelegationId);

            var result = await context.DelegationEntity.FirstOrDefaultAsync(d => d.DelegationId == delegation.DelegationId);

            Assert.NotNull(result);
            Assert.Equal(DelegationStatus.Disabled.ToString().ToLower(), result.Status);

            var resultRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == account.AccountId && r.ContactId == contact.ContactId);
            Assert.NotNull(resultRole);
        }
    }

    [Fact]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsValidAndRoleIsDelegation_ShouldDeleteDelegationAndRole()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accounts = _fixture.CreateMany<AccountEntity>(2);
            context.AccountEntity.AddRange(accounts);
            await context.SaveChangesAsync();

            var contact = _fixture.Create<ContactEntity>();
            context.ContactEntity.Add(contact);
            await context.SaveChangesAsync();

            var delegation = new DelegationEntity
            {
                Delegatee = contact,
                StartDate = DateTime.UtcNow,
                Status = "enabled",
                Account = accounts.ToList()
            };
            context.DelegationEntity.Add(delegation);
            await context.SaveChangesAsync();

            var roleIsDelegation = new RoleEntity
            {
                Account = accounts.First(),
                Contact = contact,
                IsDelegation = true,
            };
            var roleIsNotDelegation = new RoleEntity
            {
                Account = accounts.ElementAt(1),
                Contact = contact,
                IsDelegation = false,
            };
            context.RoleEntity.Add(roleIsDelegation);
            context.RoleEntity.Add(roleIsNotDelegation);
            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            await repository.DeleteDelegationAsync(delegation.DelegationId);
            var resultDelegation = await context.DelegationEntity.FirstOrDefaultAsync(d => d.DelegationId == delegation.DelegationId);

            Assert.NotNull(resultDelegation);
            Assert.Equal(DelegationStatus.Disabled.ToString().ToLower(), resultDelegation.Status);

            var resultRoleDeleted = await context.RoleEntity.FirstOrDefaultAsync(r => r.RoleId == roleIsDelegation.RoleId);
            Assert.Null(resultRoleDeleted);

            var resultRoleNotDeleted = await context.RoleEntity.FirstOrDefaultAsync(r => r.RoleId == roleIsNotDelegation.RoleId);
            Assert.NotNull(resultRoleNotDeleted);
        }
    }

    [Fact]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsInvalid_ShouldThrowNotFoundException()
    {
        var repository = new DelegationRepository(new AccountContext(_dbContextOptions));

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.DeleteDelegationAsync(1));

        Assert.Equal(Errors.NotFoundDelegationCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundDelegationMessage, 1), result.Message);
    }

    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_WhenAccountIdIsValid_ShouldReturnDelegations()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var accountId = 18;
            var tAccount = new AccountEntity
            {
                AccountId = accountId,
                AccountNumber = "00001114455",
                CreatedBy = "UnitTest@kpmg.fr",
                Email = "account-mail@kpmg.fr",
                LegalName = "Pulse",
                SourceAccountNumber = "IBS",
            };
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            var expectedDelegationsResult = new List<Delegation>();

            var delegationStatus = new List<string> { "pending", "enabled", "disabled" };
            for (var i = 1; i <= 10; i++)
            {
                var delegatorId = i * 10;
                var delegateeId = i * 110;
                context.ContactEntity.Add(new ContactEntity
                {
                    ContactId = delegatorId,
                    Email = $"Contact-mail-{delegatorId}@kpmg.fr",
                    FirstName = $"Contact-FN-{delegatorId}",
                    LastName = $"Contact-LT-{delegatorId}",
                    Type = "customer",
                    Status = "Declared",
                    PersonaName = "Collaborateur ESC",
                    Office = "Paris",
                    CreationDate = DateTime.UtcNow,
                });

                context.ContactEntity.Add(new ContactEntity
                {
                    ContactId = delegateeId,
                    Email = $"Contact-mail-{delegateeId}@kpmg.fr",
                    FirstName = $"Contact-FN-{delegateeId}",
                    LastName = $"Contact-LT-{delegateeId}",
                    Type = "customer",
                    Status = "Declared",
                    PersonaName = "Collaborateur ESC",
                    Office = "Paris",
                    CreationDate = DateTime.UtcNow,
                });

                await context.SaveChangesAsync();

                Random random = new Random();
                int randomStatusindex = random.Next(delegationStatus.Count);

                // Try create a delegation
                var tDelegation = new DelegationEntity
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = delegateeId,
                    Status = delegationStatus[randomStatusindex],
                    Note = $"Note de {delegatorId}",
                    Account = new List<AccountEntity>
                    {
                        tAccount
                    }
                };

                await context.DelegationEntity.AddRangeAsync(tDelegation);

                expectedDelegationsResult.Add(tDelegation.ToDelegation() !);
            }

            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            // Act
            var delegationList = await repository.GetAccountDelegationsHistoryAsync(accountId);

            // Assert
            Assert.Equal(10, delegationList.Count);
            delegationList.Should().BeEquivalentTo(expectedDelegationsResult);
        }
    }
}
