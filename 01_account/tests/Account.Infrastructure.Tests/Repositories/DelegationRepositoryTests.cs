// <copyright file="DelegationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;
using InvalidOperationException = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

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
          .EnableSensitiveDataLogging()
          .Options;
    }

    [Fact]
    public async Task CreateDelegationAsync_ShouldThrowNotFoundException_WhenNoExistingContactsFound()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

        CreateDelegationRequest createDelegationRequest = _fixture
            .Build<CreateDelegationRequest>()
            .Create();

        List<CreateRoleRequest> roleRequestList = _fixture
            .Build<CreateRoleRequest>()
            .CreateMany(3)
            .ToList();

        using (var context = new AccountContext(options))
        {
            var repos = new DelegationRepository(context);

            var action = async () => await repos.CreateDelegationAsync(createDelegationRequest, roleRequestList);

            var exception = await action.Should().ThrowAsync<NotFoundException>();
            var contactIds = createDelegationRequest.DelegationDetails.Select(x => x.DelegateeId).ToList();
            contactIds.Add(createDelegationRequest.DelegatorId);

            exception.WithMessage(Errors.NotFoundContactsMessage);
            exception.Which.Code.Should().Be(Errors.NotFoundContactsCode);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_ShouldThrowNotFoundException_WhenNoExistingAccountsFound()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

        CreateDelegationRequest createDelegationRequest = _fixture
            .Build<CreateDelegationRequest>()
            .Create();

        List<CreateRoleRequest> roleRequestList = _fixture
            .Build<CreateRoleRequest>()
            .CreateMany(3)
            .ToList();

        List<ContactEntity> contactEntities = new List<ContactEntity>();

        foreach (var item in createDelegationRequest.DelegationDetails)
        {
            ContactEntity contact = _fixture.Build<ContactEntity>()
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleEntity)
                .With(c => c.IsActive, true)
                .With(x => x.ContactId, item.DelegateeId)
                .Create();

            contactEntities.Add(contact);
        }

        var contactDelegator = _fixture.Build<ContactEntity>()
            .With(x => x.ContactId, createDelegationRequest.DelegatorId)
            .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleEntity)
            .Create();

        contactEntities.Add(contactDelegator);

        using (var context = new AccountContext(options))
        {
            context.ContactEntity.AddRange(contactEntities);
            context.SaveChanges();

            var repos = new DelegationRepository(context);

            var action = async () => await repos.CreateDelegationAsync(createDelegationRequest, roleRequestList);

            var exception = await action.Should().ThrowAsync<NotFoundException>();

            exception.WithMessage(Errors.NotFoundAccountsMessage);
            exception.Which.Code.Should().Be(Errors.NotFoundAccountsCode);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegationAndRole()
    {
        // Run the test against one instance of the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 123)
                .With(c => c.IsActive, true)
                .Create();
            var tDelegatee = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 456)
                .With(c => c.IsActive, true)
                .Create();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator, tDelegatee });

            // Create Role for Delegator
            var roleDelegator = _fixture.Build<RoleEntity>()
                .With(c => c.Account, tAccount)
                .With(c => c.AccountId, tAccount.AccountId)
                .With(c => c.Contact, tDelegator)
                .With(c => c.ContactId, tDelegator.ContactId)
                .Create();
            context.RoleEntity.Add(roleDelegator);
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
                        IsRoleToCreate = true,
                        IsAutomaticDelegation = false
                    },
                },
                AccountIds = new List<int> { roleDelegator.AccountId },
                IsFullDelegation = true
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

            var result = await repository.CreateDelegationAsync(createDelegation, roles);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(tAccount.AccountId, result.First().AccountId);
            Assert.Equal(tDelegatee.ContactId, result.First().ContactId);

            var createdDelegation = await context
                .DelegationEntity
                .FirstOrDefaultAsync(d => d.DelegatorId == tDelegator.ContactId
                && d.DelegateeId == tDelegatee.ContactId
                && d.Account.FirstOrDefault(a => a.AccountId == tAccount.AccountId) != null);

            Assert.NotNull(createdDelegation);
            Assert.True(createdDelegation.IsFullDelegation);
            Assert.False(createdDelegation.IsAutomaticDelegation);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValidAndExistingRole_ShouldCreateDelegation()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>()
                .With(c => c.IsActive, true)
                .With(c => c.ContactId, 124)
                .Create();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Create role
            var existingRole = _fixture.Build<RoleEntity>()
                .With(r => r.Account, tAccount)
                .With(r => r.Contact, tDelegatee)
                .With(r => r.AccountId, tAccount.AccountId)
                .With(r => r.ContactId, 124)
                .Create();
            context.RoleEntity.Add(existingRole);
            await context.SaveChangesAsync();

            // Create Role for Delegator
            var roleDelegator = _fixture.Build<RoleEntity>()
                .With(c => c.Account, tAccount)
                .With(c => c.AccountId, tAccount.AccountId)
                .With(c => c.Contact, tDelegator)
                .With(c => c.ContactId, tDelegator.ContactId)
                .Create();
            context.RoleEntity.Add(roleDelegator);
            await context.SaveChangesAsync();

            // new context created because it generates a context tracking error
            using (var newContext = new AccountContext(_dbContextOptions))
            {
                var repository = new DelegationRepository(newContext);
                var createDelegation = new CreateDelegationRequest()
                {
                    DelegatorId = tDelegator.ContactId,
                    DelegationDetails = new List<DelegationDetails>
                {
                    new()
                    {
                        DelegateeId = 124,
                        StartDate = DateTime.UtcNow,
                        Status = "enabled",
                        IsRoleToCreate = true,
                        IsAutomaticDelegation = true
                    },
                },
                    AccountIds = new List<int> { tAccount.AccountId }
                };
                var roles = new List<CreateRoleRequest>
            {
                new()
                {
                    AccountId = tAccount.AccountId,
                    ContactId = 124,
                    IsFavorite = false,
                    IsSignatory = false,
                    IsDelegation = true,
                }
            };

                await repository.CreateDelegationAsync(createDelegation, roles);

                var createdDelegation = await context
                    .DelegationEntity
                    .FirstOrDefaultAsync(d => d.DelegatorId == tDelegator.ContactId
                    && d.DelegateeId == 124
                    && d.Account.FirstOrDefault(a => a.AccountId == tAccount.AccountId) != null);

                Assert.NotNull(createdDelegation);

                var createdRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == tAccount.AccountId && r.ContactId == 124);
                createdRole.Should().NotBeNull();
                createdRole.Should().BeEquivalentTo(existingRole);
            }
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
            var contacts = new List<int> { tDelegatee.ContactId, 0 };
            var action = async () => await repository.CreateDelegationAsync(createDelegation, null!);
            var result = await action.Should().ThrowAsync<NotFoundException>();
            result.Which.Code.Should().Be(Errors.NotFoundContactsCode);
            result.WithMessage(Errors.NotFoundContactsMessage);
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

            var contacts = createDelegation.DelegationDetails.Select(x => x.DelegateeId).ToList();
            contacts.Add(createDelegation.DelegatorId);

            var action = async () => await repository.CreateDelegationAsync(createDelegation, null!);
            var result = await action.Should().ThrowAsync<NotFoundException>();
            result.Where(x => x.Code == Errors.NotFoundContactsCode);
            result.WithMessage(Errors.NotFoundContactsMessage);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenAccountNotExists_ShouldThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
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
    public async Task CreateDelegationAsync_WhenDontHaveRight_ShouldThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
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
                AccountIds = new List<int> { tAccount.AccountId }
            };

            var result = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.CreateDelegationAsync(createDelegation, null!));
            Assert.Equal(Errors.DontHaveRightAccountsCode, result.Code);
            Assert.Equal(Errors.DontHaveRightAccountsMessage, result.Message);
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
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
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
    public async Task GetContactDelegationsAsync_WhenDelegationStatusIsDisabled_ShouldReturnEmpty()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            await context.AccountEntity.AddAsync(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            await context.ContactEntity.AddRangeAsync(new List<ContactEntity> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new DelegationEntity
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "Disabled",
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

            Assert.Equal(0, contactDelegations.Count);
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
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var anotherDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            await context.ContactEntity.AddRangeAsync(new List<ContactEntity> { tDelegator, tDelegatee, anotherDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new DelegationEntity()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "Pending",
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
                Status = "Pending",
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
    public async Task GetDelegationsAsync_WhenDelegationStatusIsDisabled_ShouldReturnEmpty()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<AccountEntity>();
            await context.AccountEntity.AddAsync(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var anotherDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            await context.ContactEntity.AddRangeAsync(new List<ContactEntity> { tDelegator, tDelegatee, anotherDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new DelegationEntity()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "Disabled",
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
                Status = "Disabled",
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

            Assert.Equal(0, delegationList.Count);
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

            var contact = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            context.ContactEntity.Add(contact);
            await context.SaveChangesAsync();

            var delegation = new DelegationEntity
            {
                Delegatee = contact,
                StartDate = DateTime.UtcNow,
                Status = "Enabled",
                Account = accounts.ToList()
            };
            var otherDelegation = new DelegationEntity
            {
                Delegatee = contact,
                StartDate = DateTime.UtcNow,
                Status = "Disabled",
                Account = accounts.ToList()
            };
            var thirdDelegation = new DelegationEntity
            {
                Delegatee = contact,
                StartDate = DateTime.UtcNow,
                Status = "Enabled",
                Account = accounts.Skip(1).ToList()
            };
            context.DelegationEntity.AddRange(new List<DelegationEntity> { delegation, otherDelegation, thirdDelegation });
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

            var resultRoleDeleted = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == roleIsDelegation.AccountId && r.ContactId == roleIsDelegation.ContactId);
            Assert.Null(resultRoleDeleted);

            var resultRoleNotDeleted = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == roleIsNotDelegation.AccountId && r.ContactId == roleIsNotDelegation.ContactId);
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

    [Theory]
    [InlineData(null!, 1, 10, 1)]
    [InlineData("FN-10", 1, 2, 1)]
    public async Task GetAccountDelegationsHistoryAsync_WithNullSearchAndAccountIdIsValid_ShouldReturnDelegations(string? search,
        int currentPage,
        int totalItems,
        int totalPage)
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .EnableSensitiveDataLogging()
          .Options;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Create Account
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10
            };
            var accountId = 18;
            var tAccount = new AccountEntity
            {
                AccountId = accountId,
                AccountNumber = "00001114455",
                CreatedBy = "UnitTest@kpmg.fr",
                Email = "account-mail@kpmg.fr",
                LegalName = "Pulse",
                IsActive = true
            };
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            var expectedDelegationsResult = new List<Delegation>();

            var delegationStatus = new List<string> { "Pending", "Enabled", "Disabled" };
            for (var i = 1; i <= 10; i++)
            {
                var delegatorId = i * 10;
                var delegateeId = i * 110;
                await context.ContactEntity.AddAsync(new ContactEntity
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

                await context.ContactEntity.AddAsync(new ContactEntity
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

                expectedDelegationsResult.Add(tDelegation.ToDelegation()!);
            }

            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            // Act
            var result = await repository.GetAccountDelegationsHistoryAsync(accountId, search, pagination);

            // Assert
            result.Items.Should().NotBeNull().And.NotBeEmpty();
            result.CurrentPage.Should().Be(currentPage);
            result.TotalItems.Should().Be(totalItems);
            result.TotalPage.Should().Be(totalPage);
        }
    }

    [Fact]
    public async Task GetContactDelegationsHistoryAsync_WithContactIdIsValid_ShouldReturnDelegations()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .EnableSensitiveDataLogging()
          .Options;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Create Account
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10
            };
            var contactId = 10;
            await context.ContactEntity.AddAsync(_fixture.Build<ContactEntity>()
                .With(a => a.ContactId, contactId)
                .Without(a => a.DelegationEntityDelegatee)
                .Without(a => a.DelegationEntityDelegator)
                .Without(a => a.RoleEntity)
                .Create());
            await context.SaveChangesAsync();

            var expectedDelegationsResult = new List<Delegation>();

            var delegationStatus = new List<string> { "Pending", "Enabled", "Disabled" };
            for (var i = 1; i <= 10; i++)
            {
                var delegateeId = _fixture.Create<int>() + i;
                var contactEntity = new ContactEntity
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
                };
                await context.ContactEntity.AddAsync(contactEntity);

                Random random = new Random();
                int randomStatusindex = random.Next(delegationStatus.Count);

                // Try create a delegation
                var tDelegation = new DelegationEntity
                {
                    DelegationId = _fixture.Create<int>() + i,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = contactId,
                    DelegateeId = delegateeId,
                    Status = delegationStatus[randomStatusindex],
                    Note = $"Note de {contactId}",
                    Account = _fixture.Build<AccountEntity>().Without(a => a.Delegation).Without(a => a.RoleEntity).CreateMany(3).ToList()
                };
                await context.DelegationEntity.AddRangeAsync(new List<DelegationEntity> { tDelegation });

                expectedDelegationsResult.Add(tDelegation.ToDelegation()!);
            }

            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            // Act
            var result = await repository.GetContactDelegationsHistoryAsync(contactId, pagination, true);

            // Assert
            result.Items.Should().NotBeNull().And.NotBeEmpty();
            result.CurrentPage.Should().Be(1);
            result.TotalItems.Should().Be(10);
            result.TotalPage.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_WithAccountIdInvalid_ShouldReturnEmptyListDelegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 1
            };

            var repository = new DelegationRepository(context);

            var result = await repository.GetAccountDelegationsHistoryAsync(0, null!, pagination);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.CurrentPage.Should().Be(1);
            result.TotalItems.Should().Be(0);
            result.TotalPage.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetContactDelegationsHistoryAsync_WithContactIdInvalid_ShouldReturnEmptyListDelegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 1
            };

            var repository = new DelegationRepository(context);

            var result = await repository.GetContactDelegationsHistoryAsync(0, pagination, true);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.CurrentPage.Should().Be(1);
            result.TotalItems.Should().Be(0);
            result.TotalPage.Should().Be(1);
        }
    }

    [Fact]
    public async Task DoesAccountExistAsync_WithExistingAccount_ShouldReturnTrue()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var account = _fixture.Create<AccountEntity>();
            context.AccountEntity.Add(account);
            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            var result = await repository.DoesAccountExistAsync(account.AccountId);

            result.Should().Be(true);
        }
    }

    [Fact]
    public async Task DoesAccountExistAsync_WithNotExistingAccount_ShouldReturnFalse()
    {
        var repository = new DelegationRepository(new AccountContext(_dbContextOptions));

        var result = await repository.DoesAccountExistAsync(It.IsAny<int>());

        result.Should().Be(false);
    }

    [Fact]
    public async Task DoesContactExistAsync_WithExistingContact_ShouldReturnTrue()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var contact = _fixture.Create<ContactEntity>();
            context.ContactEntity.Add(contact);
            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            var result = await repository.DoesContactExistAsync(contact.ContactId);

            result.Should().Be(true);
        }
    }

    [Fact]
    public async Task DoesContactExistAsync_WithNotExistingContact_ShouldReturnFalse()
    {
        var repository = new DelegationRepository(new AccountContext(_dbContextOptions));

        var result = await repository.DoesContactExistAsync(It.IsAny<int>());

        result.Should().Be(false);
    }

    [Fact]
    public async Task GetAccountIdsForFullDelegationAsync_ShouldReturnAccountIdsList()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var contact = new ContactEntity
            {
                ContactId = 1,
                Email = "jp@kpmg.fr",
                FirstName = "Jean",
                LastName = "Pierre",
                PersonaName = "Collaborator",
                Type = "collaborator",
                Status = "Declared"
            };
            context.ContactEntity.Add(contact);

            var deployment1 = new DeploymentEntity
            {
                AccountId = 1,
                Status = 1
            };
            var deployment2 = new DeploymentEntity
            {
                AccountId = 2,
                Status = 4
            };
            context.DeploymentEntity.AddRange(new List<DeploymentEntity> { deployment1, deployment2 });

            var account1 = new AccountEntity
            {
                AccountId = 1,
                AccountNumber = "1",
                LegalName = "legal",
                CreatedBy = "moi",
                DeploymentEntity = new List<DeploymentEntity> { deployment1 },
                IsActive = true,
            };
            var account2 = new AccountEntity
            {
                AccountId = 2,
                AccountNumber = "2",
                LegalName = "illegal",
                CreatedBy = "moi",
                DeploymentEntity = new List<DeploymentEntity> { deployment2 },
                IsActive = true
            };
            context.AccountEntity.AddRange(new List<AccountEntity> { account1, account2 });

            var roles = new List<RoleEntity>
            {
                new()
                {
                    AccountId = account1.AccountId,
                    ContactId = contact.ContactId
                },
                new()
                {
                    AccountId = account2.AccountId,
                    ContactId = contact.ContactId
                },
                new()
                {
                    AccountId = 3,
                    ContactId = 2
                }
            };

            context.RoleEntity.AddRange(roles);
            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            var result = await repository.GetAccountIdsForFullDelegationAsync(contact.ContactId);

            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.First().Should().Be(roles.First().AccountId);
        }
    }

    [Fact]
    public async Task GetAccountIdsForFullDelegationAsync_WithInvalidRoles_ShouldReturnEmptyList()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            var roles = _fixture.CreateMany<RoleEntity>();
            context.RoleEntity.AddRange(roles);
            await context.SaveChangesAsync();

            var repository = new DelegationRepository(context);

            var result = await repository.GetAccountIdsForFullDelegationAsync(0);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    [Theory]
    [MemberData(nameof(Contacts))]
    public async Task IsClient_ShouldReturnTrue_IfContactIdsListContainsClient(IEnumerable<int> contactIds)
    {
        using var context = new AccountContext(_dbContextOptions);

        var collab1 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();
        var collab2 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 2)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();
        var client1 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 3)
            .With(c => c.Type, ContactType.Customer.ToString())
            .Create();
        var client2 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 4)
            .With(c => c.Type, ContactType.Customer.ToString())
            .Create();
        context.ContactEntity.AddRange(new List<ContactEntity> { collab1, collab1, client1, client2 });
        await context.SaveChangesAsync();

        var repository = new DelegationRepository(context);

        var result = await repository.IsClient(contactIds);

        Assert.True(result);
    }

    public static IEnumerable<object[]> Contacts()
    {
        yield return new object[] { new List<int> { 1, 3 } };
        yield return new object[] { new List<int> { 3, 2 } };
        yield return new object[] { new List<int> { 3, 4 } };
        yield return new object[] { new List<int> { 4, 3 } };
    }

    [Fact]
    public async Task IsClient_WithNoClient_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);

        var collab1 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();
        var collab2 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 2)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Create();
        context.ContactEntity.AddRange(new List<ContactEntity> { collab1, collab1 });
        await context.SaveChangesAsync();

        var repository = new DelegationRepository(context);

        var result = await repository.IsClient(new List<int> { 1, 2 });

        Assert.False(result);
    }
}
