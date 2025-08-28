// <copyright file="RolesRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;
using InvalidOperationExceptionMiddleware = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class RolesRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public RolesRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;
    }

    [Fact]
    public async Task GetContactRolesAsync_Should_ReturnsCorrectPaging()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            var contactEntity = _fixture.Build<ContactEntity>()
                                            .With(c => c.Type, "1")
                                            .With(c => c.FirstName, "firstUser")
                                            .With(c => c.LastName, "lastUser")
                                            .With(c => c.Email, "firstLastUser@test.fr")
                                            .Create();
            var roleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, contactEntity)
                                            .With(r => r.IsSignatory, true)
                                            .CreateMany(1);
            var accountsEntity = _fixture.Build<AccountEntity>()
                .With(a => a.RoleEntity, roleEntity.ToList())
                .CreateMany(1);
            var accountId = accountsEntity.First().AccountId;
            context.AccountEntity.AddRange(accountsEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);
            var contactId = accountsEntity.First(a => a.AccountId == accountId).RoleEntity.First().ContactId;

            var accountObjects = context.AccountEntity
                                                        .AsNoTracking()
                                                        .Include(x => x.RoleEntity)
                                                        .ThenInclude(r => r.Contact)
                                                        .Include(a => a.AddressEntity)
                                                        .Include(x => x.DeploymentEntity)
                                                        .Where(a => a.RoleEntity.Any(r => r.ContactId == contactId))
                                                        .OrderBy(x => x.LegalName)
                                                        .Select(x => x.MapToAccount(contactId));

            Paging<AccountModel> accountPaging = new Paging<AccountModel>()
            {
                CurrentPage = 1,
                Items = accountObjects!,
                TotalItems = accountObjects.Count(),
                TotalPage = 1
            };

            // Act
            var accounts = await rolesRepository.GetContactRolesAsync(contactId, pagination);

            // Assert
            var accountExpect = JsonConvert.SerializeObject(accountPaging);
            var accountReceived = JsonConvert.SerializeObject(accounts);
            Assert.Equal(accountExpect, accountReceived);
        }
    }

    [Fact]
    public async Task GetContactRolesAsync_WithNotExistingContactId_ShouldThrowNotFoundException()
    {
        using (var accountContext = new AccountContext(_dbContextOptions))
        {
            var contactId = 999;
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 15
            };

            var repository = new RoleRepository(accountContext);

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetContactRolesAsync(contactId, pagination));

            Assert.Equal(Errors.NotFoundContactCode, result.Code);
            Assert.Equal(string.Format(Errors.NotFoundContactMessage, contactId), result.Message);
        }
    }

    [Fact]
    public async Task GetSignatoryAsync_ShouldReturnCorrect()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountMock = _fixture.Build<AccountEntity>()
                .With(a => a.IsActive, true)
                .Without(a => a.RoleEntity)
                .Without(a => a.RoleLabelEntity)
                .Without(a => a.Delegation)
                .Create();
            var contact = _fixture.Build<ContactEntity>()
                .With(c => c.IsActive, true)
                .Without(c => c.RoleEntity)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .CreateMany(2);
            var signatory = new RoleEntity
            {
                AccountId = accountMock.AccountId,
                Account = accountMock,
                ContactId = contact.First().ContactId,
                IsSignatory = true,
                Contact = contact.First()
            };
            var nonSignatory = new RoleEntity
            {
                AccountId = accountMock.AccountId,
                Account = accountMock,
                ContactId = contact.ElementAt(1).ContactId,
                IsSignatory = false,
                Contact = contact.ElementAt(1)
            };
            var rolesMock = new List<RoleEntity> { signatory, nonSignatory };
            context.AccountEntity.Add(accountMock);
            context.ContactEntity.AddRange(contact);
            context.RoleEntity.AddRange(rolesMock);
            await context.SaveChangesAsync();

            var rolesRepository = new RoleRepository(context);
            var data = rolesMock.Where(r => r.IsSignatory!.Value).ToList();
            var resultExpected = new List<Contact> { signatory.MapToContact()! };

            // Act
            var roles = await rolesRepository.GetSignatoryAsync(data.First().AccountId);

            // Assert
            Assert.Equivalent(resultExpected, roles);
        }
    }

    [Fact]
    public async Task GetSignatoryAsync_WithNotExistingAccountId_ShouldThrowNotFoundException()
    {
        var repository = new RoleRepository(new AccountContext(_dbContextOptions));

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetSignatoryAsync(999));

        Assert.Equal(Errors.NotFoundAccountCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundAccountMessage, 999), result.Message);
    }

    [Fact]
    public async Task CreateRoleAsync_WithValidRequest_ShouldReturnRolesCreated()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = true,
            IsSignatory = false,
        };

        var deployment = new DeploymentEntity
        {
            Status = 1
        };

        using var accountContext = new AccountContext(_dbContextOptions);

        accountContext.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true
        });
        accountContext.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "customer",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        accountContext.DelegationEntity.Add(new DelegationEntity
        {
            DelegatorId = contactId,
            DelegateeId = 2,
            StartDate = DateTime.UtcNow,
            CreationDate = DateTime.UtcNow,
            IsAutomaticDelegation = true,
            Status = "enabled"
        });

        await accountContext.SaveChangesAsync();

        var rolesRepository = new RoleRepository(accountContext);

        // Act
        var result = await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateRoleAsync_UsingValidEmail_ShouldReturnRolesCreated()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = 0,
            Email = "Contact-mail@kpmg.fr",
            IsFavorite = true,
            IsSignatory = false,
        };

        var deployment = new DeploymentEntity
        {
            Status = 1
        };

        using var accountContext = new AccountContext(_dbContextOptions);

        accountContext.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true
        });
        accountContext.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "customer",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        accountContext.DelegationEntity.Add(new DelegationEntity
        {
            DelegatorId = contactId,
            DelegateeId = 2,
            StartDate = DateTime.UtcNow,
            CreationDate = DateTime.UtcNow,
            IsAutomaticDelegation = true,
            Status = "enabled"
        });

        await accountContext.SaveChangesAsync();

        var rolesRepository = new RoleRepository(accountContext);

        // Act
        var result = await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contactId, result.ContactId);
    }

    [Fact]
    public async Task CreateRoleAsync_WithInValidAccount_ShouldThrowsNotFoundException()
    {
        // Arrange
        const int invalidAccountId = 100;
        const int contactId = 456;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = invalidAccountId,
            ContactId = contactId,
            IsFavorite = true,
            IsSignatory = false,
        };

        using var accountContext = new AccountContext(_dbContextOptions);
        accountContext.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "customer",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
        });

        await accountContext.SaveChangesAsync();

        var rolesRepository = new RoleRepository(accountContext);

        // Act
        Func<Task> action = async () => await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal($"L'entité avec l'identifiant {invalidAccountId} est introuvable", exception.Message);
    }

    [Fact]
    public async Task CreateRoleAsync_WithInValidContact_ShouldThrowsNotFoundException()
    {
        // Arrange
        const int accountId = 123;
        const int invalidContactId = 456;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = invalidContactId,
            IsFavorite = true,
            IsSignatory = false,
        };

        var deployment = new DeploymentEntity
        {
            Status = 1
        };

        using var accountContext = new AccountContext(_dbContextOptions);

        accountContext.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true
        });

        await accountContext.SaveChangesAsync();

        var rolesRepository = new RoleRepository(accountContext);

        // Act
        Func<Task> action = async () => await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal($"Le contact avec l'identifiant {invalidContactId} est introuvable", exception.Message);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnOk()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var roleMock = _fixture.Create<RoleEntity>();
            roleMock.IsSignatory = true;
            context.RoleEntity.Add(roleMock);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act
            await rolesRepository.UpdateRoleSignatoryAsync(roleMock.AccountId, roleMock.ContactId, false);
            var roleObjects = context.RoleEntity
                                    .Where(x => x.ContactId == roleMock.ContactId && x.AccountId == roleMock.AccountId)
                                    .Select(x => x);
            var role = await roleObjects.FirstOrDefaultAsync();

            // Assert
            Assert.Equal(false, role!.IsSignatory);
        }
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsync_WithExistingRole_ShouldUpdateIsCustomerRelationAndSetActionLevelTo4()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = true;

        using var context = new AccountContext(_dbContextOptions);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = false,
            ActionLevel = 0
        };
        context.ContactEntity.Add(contact);
        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation);

        // Assert
        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
        Assert.Equal(4, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsyncToFalse_WithExistingRoleAndNoRoleLabel_ShouldUpdateIsCustomerRelationAndSetActionLevelTo1()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = false;

        using var context = new AccountContext(_dbContextOptions);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = true,
            ActionLevel = 4
        };
        context.ContactEntity.Add(contact);
        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation, (int)ActionLevelType.Observator);

        // Assert
        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
        Assert.Equal(1, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsyncToFalse_WithExistingRoleAndRoleLabel_ShouldUpdateIsCustomerRelationAndSetActionLevelTo3()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = false;

        using var context = new AccountContext(_dbContextOptions);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = true,
            ActionLevel = 4
        };
        var roleLabel = new RoleLabelEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            LabelId = 1
        };
        context.ContactEntity.Add(contact);
        context.RoleEntity.Add(roleEntity);
        context.RoleLabelEntity.Add(roleLabel);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation, (int)ActionLevelType.Observator);

        // Assert
        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
        Assert.Equal(3, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsync_WithNonExistingRole_ShouldThrowNotFoundException()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = true;

        using var context = new AccountContext(_dbContextOptions);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal("ACC004", exception.Code);
        Assert.Equal("Le contact avec l'identifiant 456 n'a aucun role sur l'account 123", exception.Message);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsync_WithNoChange_ShouldNotUpdateDatabase()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool isCustomerRelation = true;

        using var context = new AccountContext(_dbContextOptions);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, ContactType.Collaborator.ToString())
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = isCustomerRelation
        };
        context.ContactEntity.Add(contact);
        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();

        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, isCustomerRelation);

        // Assert
        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(isCustomerRelation, updatedRole.IsCustomerRelation);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsync_WithClient_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = true;

        using var context = new AccountContext(_dbContextOptions);
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, ContactType.Customer.ToString())
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();

        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationExceptionMiddleware>(() => action());
        Assert.Equal("ACC035", exception.Code);
        Assert.Equal("Impossible d'ajouter des libellés pour un client.", exception.Message);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnNotFound()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var rolesRepository = new RoleRepository(context);

            // Act
            Task RoleUpdate() => rolesRepository.UpdateRoleSignatoryAsync(1, 1, false);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(RoleUpdate);
        }
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldDeleteRole()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountMock = _fixture.Create<AccountEntity>();
            var contactMock = _fixture.Build<ContactEntity>()
                .With(x => x.IsActive, true)
                .CreateMany();
            context.AccountEntity.Add(accountMock);
            context.ContactEntity.AddRange(contactMock);
            await context.SaveChangesAsync();

            var roleRepository = new RoleRepository(context);
            await roleRepository.CreateRoleAsync(new CreateRoleRequest
            {
                AccountId = accountMock.AccountId,
                ContactId = contactMock.First().ContactId,
                IsFavorite = true,
                IsSignatory = false
            });

            await roleRepository.CreateRoleAsync(new CreateRoleRequest
            {
                AccountId = accountMock.AccountId,
                ContactId = contactMock.Last().ContactId,
                IsFavorite = true,
                IsSignatory = true
            });
            var accountRepository = new AccountRepository(context);

            var criteria = new SearchContactsAccountCriteria
            {
                Type = It.IsAny<ContactType>(),
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 4
            };

            var rolesBefore = await accountRepository.GetContactsAccountAsync(accountMock.AccountId, criteria, pagination);

            // Act
            await roleRepository.DeleteRoleAsync(accountMock.AccountId, contactMock.First().ContactId);

            // Assert
            var roles = await accountRepository.GetContactsAccountAsync(accountMock.AccountId, criteria, pagination);
            Assert.Equal(rolesBefore.TotalItems - 1, roles.TotalItems);
        }
    }

    [Theory]
    [InlineData(null!, "notfound@test.fr")]
    [InlineData(1, null!)]
    public async Task CheckRoleExistsAsync_ShouldReturnFalse_IfContactDoesNotExist(int? contactId, string? email)
    {
        // Arrange: Initialize the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            var contactEntity = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 2)
                .With(c => c.Type, "2")
                .With(c => c.FirstName, "firstUser")
                .With(c => c.LastName, "lastUser")
                .With(c => c.Email, "firstLastUser@test.fr")
                .Create();
            var roleEntity = _fixture.Build<RoleEntity>()
                .With(r => r.Contact, contactEntity)
                .With(r => r.IsSignatory, true)
                .CreateMany(1);
            var accountsEntity = _fixture.Build<AccountEntity>()
                .With(a => a.RoleEntity, roleEntity.ToList())
                .CreateMany(1);

            context.AccountEntity.AddRange(accountsEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act: Call the CheckRoleExistsAsync method with the defined inputs
            var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(2, contactId, null, email);

            // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
            Assert.False(contactHasRoleOnAccount);
        }
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ShouldReturnTrue_IfContactHasRoleOnSpecifiedAccount()
    {
        // Arrange: Initialize the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            var firstCustomer = _fixture.Build<ContactEntity>()
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "Test1")
                                            .With(c => c.LastName, "Test1")
                                            .With(c => c.Email, "test1@test.fr")
                                            .With(c => c.IsActive, true)
                                            .Create();
            var secondCustomer = _fixture.Build<ContactEntity>()
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "Test2")
                                            .With(c => c.LastName, "Test2")
                                            .With(c => c.Email, "test2@test.fr")
                                            .With(c => c.IsActive, true)
                                            .Create();
            var firstRoleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, firstCustomer)
                                            .With(r => r.IsSignatory, true)
                                            .CreateMany(1);
            var secondRoleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, secondCustomer)
                                            .CreateMany(1);

            var accountsEntity = _fixture.Build<AccountEntity>()
                .With(a => a.RoleEntity, firstRoleEntity.Concat(secondRoleEntity).ToList())
                .With(a => a.IsActive, true)
                .CreateMany(1);
            var accountId = accountsEntity.First().AccountId;

            context.AccountEntity.AddRange(accountsEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act: Call the CheckRoleExistsAsync method with the defined inputs
            var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(firstCustomer.ContactId, secondCustomer.ContactId, accountId, secondCustomer.Email);

            // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
            Assert.True(contactHasRoleOnAccount);
        }
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ShouldReturnTrue_IfContactHasRoleOnAccountOfPrimaryContact()
    {
        // Arrange: Initialize the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            var firstCustomer = _fixture.Build<ContactEntity>()
                                            .With(c => c.IsActive, true)
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "Test1")
                                            .With(c => c.LastName, "Test1")
                                            .With(c => c.Email, "test1@test.fr")
                                            .Create();
            var secondCustomer = _fixture.Build<ContactEntity>()
                                            .With(c => c.IsActive, true)
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "Test2")
                                            .With(c => c.LastName, "Test2")
                                            .With(c => c.Email, "test2@test.fr")
                                            .Create();
            var firstRoleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, firstCustomer)
                                            .With(r => r.IsSignatory, true)
                                            .CreateMany(1);
            var secondRoleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, secondCustomer)
                                            .CreateMany(1);

            var firstAccount = _fixture.Build<AccountEntity>()
                .With(a => a.IsActive, true)
                .With(a => a.RoleEntity, firstRoleEntity.Concat(secondRoleEntity).ToList())
                .CreateMany(1);

            context.AccountEntity.AddRange(firstAccount);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act: Call the CheckRoleExistsAsync method with the defined inputs
            var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(firstCustomer.ContactId, secondCustomer.ContactId, null, secondCustomer.Email);

            // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
            Assert.True(contactHasRoleOnAccount);
        }
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ShouldReturnFalse_IfContactHasRoleOnDifferentAccount()
    {
        // Arrange: Initialize the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            var firstCustomer = _fixture.Build<ContactEntity>()
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "Test1")
                                            .With(c => c.LastName, "Test1")
                                            .With(c => c.Email, "test1@test.fr")
                                            .Create();
            var secondCustomer = _fixture.Build<ContactEntity>()
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "Test2")
                                            .With(c => c.LastName, "Test2")
                                            .With(c => c.Email, "test2@test.fr")
                                            .Create();
            var firstRoleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, firstCustomer)
                                            .With(r => r.IsSignatory, true)
                                            .CreateMany(1);
            var secondRoleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, secondCustomer)
                                            .CreateMany(1);

            var firstAccount = _fixture.Build<AccountEntity>()
                .With(a => a.RoleEntity, firstRoleEntity.Concat(secondRoleEntity).ToList())
                .CreateMany(1);
            var secondAccount = _fixture.Build<AccountEntity>()
                .With(a => a.RoleEntity, firstRoleEntity.ToList())
                .CreateMany(1);
            var accountId = secondAccount.First().AccountId;

            context.AccountEntity.AddRange(firstAccount);
            context.AccountEntity.AddRange(secondAccount);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act: Call the CheckRoleExistsAsync method with the defined inputs
            var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(firstCustomer.ContactId, secondCustomer.ContactId, accountId, secondCustomer.Email);

            // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
            Assert.False(contactHasRoleOnAccount);
        }
    }

    [Fact]
    public async Task IsContactHasRoleInAccount_ShouldReturnTrue_IfContactHaveRole()
    {
        // Arrange: Initialize the context
        using (var context = new AccountContext(_dbContextOptions))
        {

            var accountEntity = new AccountEntity
            {
                AccountId = 1,
                AccountNumber = "1234",
                CreatedBy = "Akuiteo",
                LegalName = "Legal Name"
            };

            var contactEntity = new ContactEntity
            {
                ContactId = 1,
                FirstName = "Marc",
                LastName = "Dibeh",
                ContactGlobalUniqueId = Guid.NewGuid(),
                Email = "mdibe@rydge.fr",
                PersonaName = "Marc DIBEH",
                Type = "Collaborateur"
            };

            var roleEntity = new RoleEntity
            {
                AccountId = 1,
                ContactId = 1,
                IsSignatory = true
            };

            context.AccountEntity.Add(accountEntity);
            context.ContactEntity.Add(contactEntity);
            context.RoleEntity.Add(roleEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act: Call the CheckRoleExistsAsync method with the defined inputs
            var contactHasRoleOnAccount = await rolesRepository.IsContactHasRoleOnAccount(contactEntity.ContactId, 1, accountEntity.AccountNumber);

            // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
            Assert.True(contactHasRoleOnAccount);
        }
    }

    [Fact]
    public async Task IsContactHasRoleInAccount_ShouldReturnFalse_IfContactHaventRole()
    {
        // Arrange: Initialize the context
        using (var context = new AccountContext(_dbContextOptions))
        {
            var contactEntity = _fixture.Build<ContactEntity>()
                                            .With(c => c.Type, "2")
                                            .With(c => c.FirstName, "firstUser")
                                            .With(c => c.LastName, "lastUser")
                                            .With(c => c.Email, "firstLastUser@test.fr")
                                            .Create();
            var roleEntity = _fixture.Build<RoleEntity>()
                                            .With(r => r.Contact, contactEntity)
                                            .With(r => r.IsSignatory, true)
                                            .CreateMany(1);
            var accountsEntity = _fixture.Build<AccountEntity>()
                .With(a => a.RoleEntity, roleEntity.ToList())
                .CreateMany(1);

            context.AccountEntity.AddRange(accountsEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act: Call the CheckRoleExistsAsync method with the defined inputs
            var contactHasRoleOnAccount = await rolesRepository.IsContactHasRoleOnAccount(contactEntity.ContactId, null, "wrongAccountNumber");

            // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
            Assert.False(contactHasRoleOnAccount);
        }
    }
}
