// <copyright file="DelegationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;
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
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());

        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .EnableSensitiveDataLogging()
          .Options;
    }

    [Fact]
    public async Task CreateDelegationAsync_ShouldThrowNotFoundException_WhenNoExistingContactsFound()
    {
        var contactId = 25;
        CreateDelegationRequest createDelegationRequest = _fixture
            .Build<CreateDelegationRequest>()
            .Create();

        List<CreateRoleRequest> roleRequestList = _fixture
            .Build<CreateRoleRequest>()
            .CreateMany(3)
            .ToList();

        using (var context = new AccountContext(_dbContextOptions))
        {
            var repos = new DelegationRepository(context);

            var action = async () => await repos.CreateDelegationAsync(contactId, createDelegationRequest, roleRequestList);

            var exception = await action.Should().ThrowAsync<NotFoundException>();

            exception.WithMessage("Un des contacts est introuvable");
            exception.Which.Code.Should().Be("ACC013");
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_ShouldThrowNotFoundException_WhenNoExistingAccountsFound()
    {
        CreateDelegationRequest createDelegationRequest = _fixture
            .Build<CreateDelegationRequest>()
            .Create();

        var contactId = createDelegationRequest.DelegationDetails
            .Select(d => d.DelegateeId)
            .Append(0)
            .Max() + 1;

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
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(c => c.IsActive, true)
                .With(x => x.ContactId, item.DelegateeId)
                .Create();

            contactEntities.Add(contact);
        }

        var contactDelegator = _fixture.Build<ContactEntity>()
            .With(x => x.ContactId, contactId)
            .With(x => x.IsActive, true)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .Without(x => x.RoleEntity)
            .Without(x => x.RoleLabelEntityContact)
            .Without(x => x.RoleLabelEntityCreatedByNavigation)
            .Create();

        contactEntities.Add(contactDelegator);

        using (var context = new AccountContext(_dbContextOptions))
        {
            context.ContactEntity.AddRange(contactEntities);
            context.SaveChanges();
            context.ChangeTracker.Clear();

            var repos = new DelegationRepository(context);

            var action = async () => await repos.CreateDelegationAsync(contactId, createDelegationRequest, roleRequestList);

            var exception = await action.Should().ThrowAsync<NotFoundException>();

            exception.WithMessage("Une des entités est introuvable");
            exception.Which.Code.Should().Be("ACC012");
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegationAndRole()
    {
        // Run the test against one instance of the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 25)
                .With(c => c.IsActive, true)
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.RoleLabelEntityCreatedByNavigation)
                .Create();
            var tDelegatee = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 26)
                .With(c => c.IsActive, true)
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.RoleLabelEntityCreatedByNavigation)
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
            context.ChangeTracker.Clear();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
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

            var result = await repository.CreateDelegationAsync(tDelegator.ContactId, createDelegation, roles);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(tAccount.AccountId, result.First().AccountId);
            Assert.Equal(tDelegatee.ContactId, result.First().ContactId);

            var createdDelegation = await context
                .DelegationEntity
                .Include(d => d.Account)
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
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(a => a.Delegation)
                .Without(a => a.RoleEntity)
                .Create();
            context.AccountEntity.Add(tAccount);

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>()
                .With(c => c.IsActive, true)
                .With(c => c.ContactId, 1245)
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Create();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator, tDelegatee });

            // Create role
            var existingRole = new RoleEntity
            {
                AccountId = tAccount.AccountId,
                ContactId = tDelegatee.ContactId,
            };
            context.RoleEntity.Add(existingRole);

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
                    DelegationDetails = new List<DelegationDetails>
                {
                    new()
                    {
                        DelegateeId = 1245,
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
                        ContactId = 1245,
                        IsFavorite = false,
                        IsSignatory = false,
                        IsDelegation = true,
                    }
                };

                await repository.CreateDelegationAsync(tDelegator.ContactId, createDelegation, roles);

                var createdDelegation = await context
                    .DelegationEntity
                    .FirstOrDefaultAsync(d => d.DelegatorId == tDelegator.ContactId
                    && d.DelegateeId == tDelegatee.ContactId
                    && d.Account.FirstOrDefault(a => a.AccountId == tAccount.AccountId) != null);

                Assert.NotNull(createdDelegation);

                var createdRole = await context.RoleEntity.FirstOrDefaultAsync(r => r.AccountId == tAccount.AccountId && r.ContactId == 1245);
                createdRole.Should().NotBeNull();
                createdRole.Should().BeEquivalentTo(existingRole);
                context.ChangeTracker.Clear();
            }
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenDelegatorDoesNotExist_ShouldThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegatee = _fixture.Build<ContactEntity>()
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.RoleLabelEntityCreatedByNavigation)
                .Create();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegatee });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
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
            var action = async () => await repository.CreateDelegationAsync(0, createDelegation, null!);
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
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>()
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.RoleLabelEntityCreatedByNavigation)
                .Create();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
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
            contacts.Add(tDelegator.ContactId);

            var action = async () => await repository.CreateDelegationAsync(tDelegator.ContactId, createDelegation, null!);
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
            context.ChangeTracker.Clear();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
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

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(tDelegator.ContactId, createDelegation, null!));
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
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
            context.AccountEntity.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            var tDelegatee = _fixture.Build<ContactEntity>().With(c => c.IsActive, true).Create();
            context.ContactEntity.AddRange(new List<ContactEntity> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegationRequest()
            {
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

            var result = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.CreateDelegationAsync(tDelegator.ContactId, createDelegation, null!));
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
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
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
            context.ChangeTracker.Clear();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var contactDelegations = await repository.GetContactDelegationsAsync(tDelegatee.ContactId);

            Assert.Single(contactDelegations);
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
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
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
            context.ChangeTracker.Clear();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var contactDelegations = await repository.GetContactDelegationsAsync(tDelegatee.ContactId);

            Assert.Empty(contactDelegations);
        }
    }

    [Fact]
    public async Task GetDelegationsAsync_WhenRequestIsValid_ShouldReturnDelegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
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
            context.ChangeTracker.Clear();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var delegationList = await repository.GetDelegationsAsync(tDelegator.ContactId, tDelegatee.ContactId);

            Assert.Single(delegationList);
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
            var tAccount = _fixture.Build<AccountEntity>()
                .Without(x => x.RoleEntity)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.Office)
                .Without(x => x.OfficeId)
                .Without(x => x.AddressEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Hub)
                .Without(x => x.Naf)
                .Without(x => x.OfferEligibilityEntity)
                .Without(x => x.Delegation)
                .Create();
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
            context.ChangeTracker.Clear();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var delegationList = await repository.GetDelegationsAsync(tDelegator.ContactId, tDelegatee.ContactId);

            Assert.Empty(delegationList);
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
            context.ChangeTracker.Clear();

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

    [Fact]
    public async Task DeleteDelegationAsync_ShouldDisableDuplicatesForSamePair()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(dbContextOptions);

        var delegator = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 900010)
            .With(c => c.IsActive, true)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var delegatee = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 900020)
            .With(c => c.IsActive, true)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.AddRange(delegator, delegatee);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.IsActive, true)
            .Without(a => a.Delegation)
            .Without(a => a.RoleEntity)
            .Without(a => a.RoleLabelEntity)
            .Create();
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        context.DelegationEntity.AddRange(
            new DelegationEntity
            {
                DelegationId = 905001, DelegatorId = 900010, DelegateeId = 900020,
                StartDate = DateTime.UtcNow, Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true, Account = new List<AccountEntity> { account },
            },
            new DelegationEntity
            {
                DelegationId = 905002, DelegatorId = 900010, DelegateeId = 900020,
                StartDate = DateTime.UtcNow, Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true, Account = new List<AccountEntity> { account },
            },
            new DelegationEntity
            {
                DelegationId = 905003, DelegatorId = 900010, DelegateeId = 900020,
                StartDate = DateTime.UtcNow, Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true, Account = new List<AccountEntity> { account },
            });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DelegationRepository(context);
        await repository.DeleteDelegationAsync(905001);

        var allForPair = await context.DelegationEntity
            .Where(d => d.DelegatorId == 900010 && d.DelegateeId == 900020)
            .ToListAsync();

        allForPair.Should().HaveCount(3);
        allForPair.Should().OnlyContain(d => d.Status == DelegationStatus.Disabled.ToString().ToLower());
    }

    [Fact]
    public async Task DeleteDelegationAsync_WhenNoDuplicates_ShouldNotAffectOtherDelegations()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(dbContextOptions);

        var delegator = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 910010)
            .With(c => c.IsActive, true)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var delegatee1 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 910020)
            .With(c => c.IsActive, true)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var delegatee2 = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 910030)
            .With(c => c.IsActive, true)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.AddRange(delegator, delegatee1, delegatee2);

        var account = _fixture.Build<AccountEntity>()
            .With(a => a.IsActive, true)
            .Without(a => a.Delegation)
            .Without(a => a.RoleEntity)
            .Without(a => a.RoleLabelEntity)
            .Create();
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var delegationToDelete = new DelegationEntity
        {
            DelegationId = 916001,
            DelegatorId = 910010,
            DelegateeId = 910020,
            StartDate = DateTime.UtcNow,
            Status = DelegationStatus.Enabled.ToString(),
            IsAutomaticDelegation = true,
            Account = new List<AccountEntity> { account },
        };
        var otherDelegation = new DelegationEntity
        {
            DelegationId = 916002,
            DelegatorId = 910010,
            DelegateeId = 910030,
            StartDate = DateTime.UtcNow,
            Status = DelegationStatus.Enabled.ToString(),
            IsAutomaticDelegation = true,
            Account = new List<AccountEntity> { account },
        };
        context.DelegationEntity.AddRange(delegationToDelete, otherDelegation);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DelegationRepository(context);
        await repository.DeleteDelegationAsync(916001);

        var otherResult = await context.DelegationEntity
            .FirstOrDefaultAsync(d => d.DelegationId == 916002);

        otherResult!.Status.Should().Be(DelegationStatus.Enabled.ToString());
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
                Status = "Declared",
                IsActive = true,
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
                DeploymentEntity = deployment1,
                IsActive = true,
            };
            var account2 = new AccountEntity
            {
                AccountId = 2,
                AccountNumber = "2",
                LegalName = "illegal",
                CreatedBy = "moi",
                DeploymentEntity = deployment2,
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
            context.ChangeTracker.Clear();

            var repository = new DelegationRepository(context);

            var result = await repository.GetAccountIdsForFullDelegationAsync(contact.ContactId);

            result.Should().NotBeNull();
            result.Should().Contain(2);
            result.First().Should().Be(roles.First().AccountId);
            result.ElementAt(1).Should().Be(roles.ElementAt(1).AccountId);
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
            context.ChangeTracker.Clear();

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
        context.ChangeTracker.Clear();

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
        context.ChangeTracker.Clear();

        var repository = new DelegationRepository(context);

        var result = await repository.IsClient(new List<int> { 1, 2 });

        Assert.False(result);
    }

    [Fact]
    public async Task GetContactDelegationsAsync_WithProspectAccount_ShouldExcludeProspectAccount()
    {
        using var context = new AccountContext(_dbContextOptions);

        var delegator = new ContactEntity
        {
            ContactId = 1,
            Email = "delegator@test.fr",
            FirstName = "Jean",
            LastName = "Dupont",
            PersonaName = "Jean Dupont",
            Type = ContactType.Collaborator.ToString(),
            Status = "Declared",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
        var delegatee = new ContactEntity
        {
            ContactId = 2,
            Email = "delegatee@test.fr",
            FirstName = "Paul",
            LastName = "Martin",
            PersonaName = "Paul Martin",
            Type = ContactType.Collaborator.ToString(),
            Status = "Declared",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
        var clientAccount = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-CLIENT-001",
            LegalName = "Client Account",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true,
        };
        var prospectAccount = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-PROSPECT-002",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
        };

        context.DelegationEntity.Add(new DelegationEntity
        {
            DelegationId = 1,
            Delegator = delegator,
            Delegatee = delegatee,
            StartDate = DateTime.UtcNow,
            Status = DelegationStatus.Enabled.ToString(),
            Account = new List<AccountEntity> { clientAccount, prospectAccount }
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DelegationRepository(context);

        var result = await repository.GetContactDelegationsAsync(delegatee.ContactId);

        Assert.Single(result);
        Assert.Single(result.First().Accounts);
        Assert.Equal(clientAccount.AccountId, result.First().Accounts.Single().AccountId);
    }

    [Fact]
    public async Task GetDelegationsAsync_WithProspectAccount_ShouldExcludeProspectAccount()
    {
        using var context = new AccountContext(_dbContextOptions);

        var delegator = new ContactEntity
        {
            ContactId = 10,
            Email = "delegator@test.fr",
            FirstName = "Jean",
            LastName = "Dupont",
            PersonaName = "Jean Dupont",
            Type = ContactType.Collaborator.ToString(),
            Status = "Declared",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
        var delegatee = new ContactEntity
        {
            ContactId = 20,
            Email = "delegatee@test.fr",
            FirstName = "Paul",
            LastName = "Martin",
            PersonaName = "Paul Martin",
            Type = ContactType.Collaborator.ToString(),
            Status = "Declared",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
        var clientAccount = new AccountEntity
        {
            AccountId = 3,
            AccountNumber = "ACC-CLIENT-003",
            LegalName = "Client Account",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true,
        };
        var prospectAccount = new AccountEntity
        {
            AccountId = 4,
            AccountNumber = "ACC-PROSPECT-004",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
        };

        context.DelegationEntity.Add(new DelegationEntity
        {
            DelegationId = 2,
            Delegator = delegator,
            Delegatee = delegatee,
            StartDate = DateTime.UtcNow,
            Status = DelegationStatus.Enabled.ToString(),
            Account = new List<AccountEntity> { clientAccount, prospectAccount }
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new DelegationRepository(context);

        var result = await repository.GetDelegationsAsync(delegator.ContactId, delegatee.ContactId);

        Assert.Single(result);
        Assert.Single(result.First().Accounts);
        Assert.Equal(clientAccount.AccountId, result.First().Accounts.Single().AccountId);
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_ShouldReturnActiveDelegations()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(dbContextOptions);

        var delegatorId = 800010;
        await context.ContactEntity.AddAsync(_fixture.Build<ContactEntity>()
            .With(a => a.ContactId, delegatorId)
            .With(a => a.IsActive, true)
            .Without(a => a.DelegationEntityDelegatee)
            .Without(a => a.DelegationEntityDelegator)
            .Without(a => a.RoleEntity)
            .Without(a => a.RoleLabelEntityContact)
            .Without(a => a.RoleLabelEntityCreatedByNavigation)
            .Create());
        await context.SaveChangesAsync();

        for (var i = 1; i <= 5; i++)
        {
            var delegateeId = 800100 + i;
            await context.ContactEntity.AddAsync(new ContactEntity
            {
                ContactId = delegateeId,
                Email = $"delegatee-{delegateeId}@test.fr",
                FirstName = $"FN-{delegateeId}",
                LastName = $"LN-{delegateeId}",
                Type = "customer",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true,
            });

            await context.DelegationEntity.AddAsync(new DelegationEntity
            {
                DelegationId = 801000 + i,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(i),
                DelegatorId = delegatorId,
                DelegateeId = delegateeId,
                Status = i <= 4 ? DelegationStatus.Enabled.ToString() : DelegationStatus.Disabled.ToString(),
                Note = $"Note {i}",
                IsAutomaticDelegation = i <= 2,
                Account = _fixture.Build<AccountEntity>()
                    .With(a => a.IsActive, true)
                    .Without(a => a.Delegation)
                    .Without(a => a.RoleEntity)
                    .Without(a => a.RoleLabelEntity)
                    .CreateMany(1).ToList(),
            });
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        using var readContext = new AccountContext(dbContextOptions);
        var repository = new DelegationRepository(readContext);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var allResult = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter(), pagination);

        allResult.Items.Should().HaveCount(4);
        allResult.TotalItems.Should().Be(4);
        allResult.CurrentPage.Should().Be(1);

        var automaticResult = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter { IsAutomatic = true }, pagination);

        automaticResult.Items.Should().HaveCount(2);
        automaticResult.TotalItems.Should().Be(2);

        var manualResult = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter { IsAutomatic = false }, pagination);

        manualResult.Items.Should().HaveCount(2);
        manualResult.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_WhenNoDelegations_ShouldReturnEmptyPaging()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRepository(context);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetDelegatorDelegationsAsync(
            999, new DelegationFilter(), pagination);

        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_ShouldPaginateCorrectly()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(dbContextOptions);

        var delegatorId = 820010;
        await context.ContactEntity.AddAsync(_fixture.Build<ContactEntity>()
            .With(a => a.ContactId, delegatorId)
            .With(a => a.IsActive, true)
            .Without(a => a.DelegationEntityDelegatee)
            .Without(a => a.DelegationEntityDelegator)
            .Without(a => a.RoleEntity)
            .Without(a => a.RoleLabelEntityContact)
            .Without(a => a.RoleLabelEntityCreatedByNavigation)
            .Create());
        await context.SaveChangesAsync();

        for (var i = 1; i <= 5; i++)
        {
            var delegateeId = 820200 + i;
            await context.ContactEntity.AddAsync(new ContactEntity
            {
                ContactId = delegateeId,
                Email = $"delegatee-{delegateeId}@test.fr",
                FirstName = $"FN-{delegateeId}",
                LastName = $"LN-{delegateeId}",
                Type = "customer",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true,
            });

            await context.DelegationEntity.AddAsync(new DelegationEntity
            {
                DelegationId = 822000 + i,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(i),
                DelegatorId = delegatorId,
                DelegateeId = delegateeId,
                Status = DelegationStatus.Enabled.ToString(),
                Note = $"Note {i}",
                IsAutomaticDelegation = true,
                Account = _fixture.Build<AccountEntity>()
                    .With(a => a.IsActive, true)
                    .Without(a => a.Delegation)
                    .Without(a => a.RoleEntity)
                    .Without(a => a.RoleLabelEntity)
                    .CreateMany(1).ToList(),
            });
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        using var readContext = new AccountContext(dbContextOptions);
        var repository = new DelegationRepository(readContext);

        var page1 = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter(), new Pagination { PageNumber = 1, PageSize = 2 });

        page1.Items.Should().HaveCount(2);
        page1.TotalItems.Should().Be(5);
        page1.TotalPage.Should().Be(3);
        page1.CurrentPage.Should().Be(1);

        var page2 = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter(), new Pagination { PageNumber = 2, PageSize = 2 });

        page2.Items.Should().HaveCount(2);
        page2.CurrentPage.Should().Be(2);

        var page3 = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter(), new Pagination { PageNumber = 3, PageSize = 2 });

        page3.Items.Should().HaveCount(1);
        page3.CurrentPage.Should().Be(3);
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_ShouldExcludeInactiveDelegatees()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(dbContextOptions);

        var delegatorId = 830010;
        await context.ContactEntity.AddAsync(_fixture.Build<ContactEntity>()
            .With(a => a.ContactId, delegatorId)
            .With(a => a.IsActive, true)
            .Without(a => a.DelegationEntityDelegatee)
            .Without(a => a.DelegationEntityDelegator)
            .Without(a => a.RoleEntity)
            .Without(a => a.RoleLabelEntityContact)
            .Without(a => a.RoleLabelEntityCreatedByNavigation)
            .Create());
        await context.SaveChangesAsync();

        var activeDelegateeId = 830101;
        var inactiveDelegateeId = 830102;

        await context.ContactEntity.AddRangeAsync(
            new ContactEntity
            {
                ContactId = activeDelegateeId,
                Email = "active@test.fr",
                FirstName = "Active",
                LastName = "User",
                Type = "customer",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true,
            },
            new ContactEntity
            {
                ContactId = inactiveDelegateeId,
                Email = "inactive@test.fr",
                FirstName = "Inactive",
                LastName = "User",
                Type = "customer",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = false,
            });

        await context.DelegationEntity.AddRangeAsync(
            new DelegationEntity
            {
                DelegationId = 833001,
                StartDate = DateTime.UtcNow,
                DelegatorId = delegatorId,
                DelegateeId = activeDelegateeId,
                Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true,
            },
            new DelegationEntity
            {
                DelegationId = 833002,
                StartDate = DateTime.UtcNow,
                DelegatorId = delegatorId,
                DelegateeId = inactiveDelegateeId,
                Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true,
            });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        using var readContext = new AccountContext(dbContextOptions);
        var repository = new DelegationRepository(readContext);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter { IsAutomatic = true }, pagination);

        result.TotalItems.Should().Be(1);
        result.Items!.Should().ContainSingle()
            .Which.Delegatee!.ContactId.Should().Be(activeDelegateeId);
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_ShouldReturnOneDelegationPerDelegatee()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AccountContext(dbContextOptions);

        var delegatorId = 840010;
        var delegateeId = 840101;

        await context.ContactEntity.AddRangeAsync(
            _fixture.Build<ContactEntity>()
                .With(a => a.ContactId, delegatorId)
                .With(a => a.IsActive, true)
                .Without(a => a.DelegationEntityDelegatee)
                .Without(a => a.DelegationEntityDelegator)
                .Without(a => a.RoleEntity)
                .Without(a => a.RoleLabelEntityContact)
                .Without(a => a.RoleLabelEntityCreatedByNavigation)
                .Create(),
            new ContactEntity
            {
                ContactId = delegateeId,
                Email = "delegatee@test.fr",
                FirstName = "FN",
                LastName = "LN",
                Type = "customer",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true,
            });

        await context.DelegationEntity.AddRangeAsync(
            new DelegationEntity
            {
                DelegationId = 844001,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                CreationDate = DateTime.UtcNow.AddMonths(-6),
                DelegatorId = delegatorId,
                DelegateeId = delegateeId,
                Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true,
            },
            new DelegationEntity
            {
                DelegationId = 844002,
                StartDate = DateTime.UtcNow,
                CreationDate = DateTime.UtcNow,
                DelegatorId = delegatorId,
                DelegateeId = delegateeId,
                Status = DelegationStatus.Enabled.ToString(),
                IsAutomaticDelegation = true,
            });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        using var readContext = new AccountContext(dbContextOptions);
        var repository = new DelegationRepository(readContext);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetDelegatorDelegationsAsync(
            delegatorId, new DelegationFilter { IsAutomatic = true }, pagination);

        result.TotalItems.Should().Be(1);
        result.Items!.Should().ContainSingle()
            .Which.Delegatee!.ContactId.Should().Be(delegateeId);
    }
}
