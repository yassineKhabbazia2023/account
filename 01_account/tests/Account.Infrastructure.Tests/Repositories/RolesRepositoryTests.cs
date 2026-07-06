// <copyright file="RolesRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

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
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;
    }

    [Fact]
    public async Task GetContactRolesAsync_Should_ReturnsCorrectPaging()
    {
        // Arrange
        var context = CreateSqliteContext();
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

    [Fact]
    public async Task GetContactRolesAsync_WithNotExistingContactId_ShouldThrowNotFoundException()
    {
        var context = CreateSqliteContext();
        var contactId = 999;
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 15
        };

        var repository = new RoleRepository(context);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetContactRolesAsync(contactId, pagination));

        Assert.Equal(Errors.NotFoundContactCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, contactId), result.Message);
    }

    [Fact]
    public async Task GetSignatoryAsync_ShouldReturnCorrect()
    {
        // Arrange
        var context = CreateSqliteContext();
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);
        var data = rolesMock.Where(r => r.IsSignatory!.Value).ToList();
        var resultExpected = new List<Contact> { signatory.MapToContact()! };

        // Act
        var roles = await rolesRepository.GetSignatoryAsync(data.First().AccountId);

        // Assert
        Assert.Equivalent(resultExpected, roles);
    }

    [Fact]
    public async Task GetSignatoryAsync_WithNotExistingAccountId_ShouldThrowNotFoundException()
    {
        var context = CreateSqliteContext();
        var repository = new RoleRepository(context);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetSignatoryAsync(999));

        Assert.Equal(Errors.NotFoundAccountCode, result.Code);
        Assert.Equal(string.Format(Errors.NotFoundAccountMessage, 999), result.Message);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldSetIsCustomerRelationAndActionLevelToDefault_WhenContactIsCustomer()
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
            ActionLevel = 2,
            IsCustomerRelation = true,
        };

        var deployment = new DeploymentEntity
        {
            Status = 1
        };

        var context = CreateSqliteContext();

        context.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        });
        context.ContactEntity.Add(new ContactEntity
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

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.CreateRoleAsync(roleRequest);
        var result = await context.RoleEntity.FirstAsync(r => r.AccountId == accountId && r.ContactId == contactId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.ActionLevel);
        Assert.Null(result.IsCustomerRelation);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldSetIsCustomerRelationAndActionLevelToDefault_WhenContactIsCollab()
    {
        // Arrange
        const int accountId = 456;
        const int contactId = 789;
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

        var context = CreateSqliteContext();

        context.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        });
        context.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "Collaborator",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.CreateRoleAsync(roleRequest);
        var result = await context.RoleEntity.FirstAsync(r => r.AccountId == accountId && r.ContactId == contactId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ActionLevel);
        Assert.Equal(false, result.IsCustomerRelation);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldSetIsCustomerRelationAndActionLevel_WhenContactIsCollab()
    {
        // Arrange
        const int accountId = 456;
        const int contactId = 789;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = true,
            IsSignatory = false,
            ActionLevel = 4,
            IsCustomerRelation = true,
        };

        var deployment = new DeploymentEntity
        {
            Status = 1
        };

        var context = CreateSqliteContext();

        context.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        });
        context.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "Collaborator",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.CreateRoleAsync(roleRequest);
        var result = await context.RoleEntity.FirstAsync(r => r.AccountId == accountId && r.ContactId == contactId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleRequest.ActionLevel, result.ActionLevel);
        Assert.Equal(roleRequest.IsCustomerRelation, result.IsCustomerRelation);
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

        var context = CreateSqliteContext();

        context.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        });
        context.ContactEntity.Add(new ContactEntity
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

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

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

        var context = CreateSqliteContext();
        context.ContactEntity.Add(new ContactEntity
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

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

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

        var context = CreateSqliteContext();

        context.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            DeploymentEntity = deployment,
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString()
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal($"Le contact avec l'identifiant {invalidContactId} est introuvable", exception.Message);
    }

    [Fact]
    public async Task CreateRoleWithoutAccountValidationAsync_WithValidContact_ShouldReturnRoleCreated()
    {
        // Arrange
        const int accountId = 200;
        const int contactId = 300;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = true,
            IsSignatory = false,
            IsDelegation = false,
            IncludePennylaneAccess = true,
        };

        var context = CreateSqliteContext();

        context.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "prospect-contact@kpmg.fr",
            FirstName = "Prospect-FN",
            LastName = "Prospect-LT",
            Type = "customer",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.CreateRoleWithoutAccountValidationAsync(roleRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contactId, result!.ContactId);
        Assert.Equal(accountId, result.AccountId);
    }

    [Fact]
    public async Task CreateRoleWithoutAccountValidationAsync_WithCollaboratorContact_ShouldSetIsCustomerRelationAndActionLevel()
    {
        // Arrange
        const int accountId = 201;
        const int contactId = 301;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = contactId,
            IsSignatory = false,
        };

        var context = CreateSqliteContext();

        context.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "collab-contact@kpmg.fr",
            FirstName = "Collab-FN",
            LastName = "Collab-LT",
            Type = ContactType.Collaborator.ToString(),
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.CreateRoleWithoutAccountValidationAsync(roleRequest);

        // Assert
        Assert.NotNull(result);
        Assert.False(result!.IsCustomerRelation);
        Assert.Equal((int)ActionLevelType.Observator, result.ActionLevel);
    }

    [Fact]
    public async Task CreateRoleWithoutAccountValidationAsync_WithInvalidContact_ShouldThrowsNotFoundException()
    {
        // Arrange
        const int accountId = 202;
        const int invalidContactId = 999;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = invalidContactId,
            IsSignatory = false,
        };

        var context = CreateSqliteContext();
        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.CreateRoleWithoutAccountValidationAsync(roleRequest);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal($"Le contact avec l'identifiant {invalidContactId} est introuvable", exception.Message);
    }

    [Fact]
    public async Task CreateRoleWithoutAccountValidationAsync_WithInactiveContact_ShouldThrowsNotFoundException()
    {
        // Arrange
        const int accountId = 203;
        const int contactId = 303;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = contactId,
            IsSignatory = false,
        };

        var context = CreateSqliteContext();

        context.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "inactive-contact@kpmg.fr",
            FirstName = "Inactive-FN",
            LastName = "Inactive-LT",
            Type = "customer",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = false
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.CreateRoleWithoutAccountValidationAsync(roleRequest);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal($"Le contact avec l'identifiant {contactId} est introuvable", exception.Message);
    }

    [Fact]
    public async Task CreateRoleWithoutAccountValidationAsync_WithExistingRole_ShouldThrowsConflictException()
    {
        // Arrange
        const int accountId = 204;
        const int contactId = 304;
        var roleRequest = new CreateRoleRequest
        {
            AccountId = accountId,
            ContactId = contactId,
            IsSignatory = false,
        };

        var context = CreateSqliteContext();

        context.ContactEntity.Add(new ContactEntity
        {
            ContactId = contactId,
            Email = "existing-contact@kpmg.fr",
            FirstName = "Existing-FN",
            LastName = "Existing-LT",
            Type = "customer",
            Status = "Declared",
            PersonaName = "Collaborateur ESC",
            Office = "Paris",
            CreationDate = DateTime.UtcNow,
            IsActive = true
        });

        context.RoleEntity.Add(new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = false,
            IsSignatory = false,
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.CreateRoleWithoutAccountValidationAsync(roleRequest);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => action());
        Assert.Equal($"Le role ContactId {contactId}/AccountId {accountId} existe déjà", exception.Message);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnOk()
    {
        // Arrange
        var context = CreateSqliteContext();
        var roleMock = _fixture.Create<RoleEntity>();
        roleMock.IsSignatory = true;
        context.RoleEntity.Add(roleMock);
        context.SaveChanges();
        context.ChangeTracker.Clear();

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

    [Fact]
    public async Task UpdateRoleCollaboratorInformationAsync_WithExistingRole_ShouldUpdateIsCustomerRelationAndActionLevel()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = true;

        var context = CreateSqliteContext();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = false,
            ActionLevel = 0
        };
        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation, (int)ActionLevelType.DirectClientRelation);

        // Assert
        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
        Assert.Equal((int)ActionLevelType.DirectClientRelation, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleCollaboratorInformationAsyncToFalse_WithExistingRole_ShouldUpdateIsCustomerRelationAndActionLevel()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = false;

        var context = CreateSqliteContext();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = true,
            ActionLevel = (int)ActionLevelType.DirectClientRelation
        };
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
        Assert.Equal((int)ActionLevelType.Observator, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleCollaboratorInformationAsync_WithNonExistingRole_ShouldThrowNotFoundException()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = true;

        var context = CreateSqliteContext();
        var rolesRepository = new RoleRepository(context);

        // Act
        Func<Task> action = async () => await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, newIsCustomerRelation, (int)ActionLevelType.DirectClientRelation);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => action());
        Assert.Equal(Errors.NotFoundRoleCode, exception.Code);
    }

    [Fact]
    public async Task UpdateRoleCollaboratorInformationAsync_WithNoChange_ShouldNotUpdateDatabase()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool isCustomerRelation = true;

        var context = CreateSqliteContext();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = isCustomerRelation,
            ActionLevel = (int)ActionLevelType.DirectClientRelation
        };
        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        await rolesRepository.UpdateRoleCollaboratorInformationAsync(accountId, contactId, isCustomerRelation, (int)ActionLevelType.DirectClientRelation);

        // Assert
        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(isCustomerRelation, updatedRole.IsCustomerRelation);
        Assert.Equal((int)ActionLevelType.DirectClientRelation, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task UpdateRoleCustomerRelationAsync_WithMultipleExistingRoles_ShouldUpdateAll()
    {
        // Arrange
        const int contactId = 456;
        var accountIds = new List<int> { 123, 124, 125 };
        const bool newIsCustomerRelation = true;
        var accountActionLevels = accountIds.ToDictionary(id => id, _ => (int)ActionLevelType.DirectClientRelation);

        var context = CreateSqliteContext();
        var roles = accountIds.Select(accountId => new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = false,
            ActionLevel = 0
        }).ToList();

        context.RoleEntity.AddRange(roles);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.UpdateRoleCustomerRelationAsync(contactId, newIsCustomerRelation, accountActionLevels);

        // Assert
        Assert.Equal(3, result.Succeeded.Count);
        Assert.Empty(result.Failed);

        foreach (var accountId in accountIds)
        {
            var updatedRole = await context.RoleEntity
                .FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
            Assert.NotNull(updatedRole);
            Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
            Assert.Equal((int)ActionLevelType.DirectClientRelation, updatedRole.ActionLevel);
        }
    }

    [Fact]
    public async Task UpdateRoleCustomerRelationAsync_WithMissingRoles_ShouldReturnPartialFailure()
    {
        // Arrange
        const int contactId = 456;
        const int existingAccountId = 123;
        const int missingAccountId = 999;
        var accountActionLevels = new Dictionary<int, int>
        {
            { existingAccountId, (int)ActionLevelType.DirectClientRelation },
            { missingAccountId, (int)ActionLevelType.DirectClientRelation }
        };
        const bool newIsCustomerRelation = true;

        var context = CreateSqliteContext();
        var roleEntity = new RoleEntity
        {
            AccountId = existingAccountId,
            ContactId = contactId,
            IsCustomerRelation = false,
            ActionLevel = 0
        };

        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.UpdateRoleCustomerRelationAsync(contactId, newIsCustomerRelation, accountActionLevels);

        // Assert
        Assert.Single(result.Succeeded, x => x.AccountId == existingAccountId);
        Assert.Single(result.Failed, x => x.AccountId == missingAccountId);
        Assert.Equal(Errors.NotFoundRoleCode, result.Failed.First().ErrorCode);

        var updatedRole = await context.RoleEntity
            .FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == existingAccountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
    }

    [Fact]
    public async Task UpdateRoleCustomerRelationAsync_WithAllMissingRoles_ShouldReturnAllFailed()
    {
        // Arrange
        const int contactId = 456;
        var accountIds = new List<int> { 111, 222, 333 };
        const bool newIsCustomerRelation = true;
        var accountActionLevels = accountIds.ToDictionary(id => id, _ => (int)ActionLevelType.DirectClientRelation);

        var context = CreateSqliteContext();
        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.UpdateRoleCustomerRelationAsync(contactId, newIsCustomerRelation, accountActionLevels);

        // Assert
        Assert.Empty(result.Succeeded);
        Assert.Equal(3, result.Failed.Count);
        Assert.All(result.Failed, item => Assert.Equal(Errors.NotFoundRoleCode, item.ErrorCode));
    }

    [Fact]
    public async Task UpdateRoleCustomerRelationAsyncToFalse_WithSingleExistingRole_ShouldUpdateIsCustomerRelationAndActionLevel()
    {
        // Arrange
        const int accountId = 123;
        const int contactId = 456;
        const bool newIsCustomerRelation = false;
        var accountActionLevels = new Dictionary<int, int> { { accountId, (int)ActionLevelType.Observator } };

        var context = CreateSqliteContext();
        var roleEntity = new RoleEntity
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = true,
            ActionLevel = (int)ActionLevelType.DirectClientRelation
        };
        context.RoleEntity.Add(roleEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.UpdateRoleCustomerRelationAsync(contactId, newIsCustomerRelation, accountActionLevels);

        // Assert
        Assert.Single(result.Succeeded, x => x.AccountId == accountId);
        Assert.Empty(result.Failed);

        var updatedRole = await context.RoleEntity.FirstOrDefaultAsync(x => x.ContactId == contactId && x.AccountId == accountId);
        Assert.NotNull(updatedRole);
        Assert.Equal(newIsCustomerRelation, updatedRole.IsCustomerRelation);
        Assert.Equal((int)ActionLevelType.Observator, updatedRole.ActionLevel);
    }

    [Fact]
    public async Task GetRolesByContactAndAccountIdsAsync_WithExistingRoles_ShouldReturnMappedRoles()
    {
        // Arrange
        const int contactId = 456;
        var accountIds = new List<int> { 10, 11 };

        var context = CreateSqliteContext();
        context.RoleEntity.AddRange(
            new RoleEntity { AccountId = 10, ContactId = contactId, ActionLevel = 1 },
            new RoleEntity { AccountId = 11, ContactId = contactId, ActionLevel = 4 },
            new RoleEntity { AccountId = 12, ContactId = contactId, ActionLevel = 3 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.GetRolesByContactAndAccountIdsAsync(contactId, accountIds);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, role => Assert.Equal(contactId, role.ContactId));
        Assert.Contains(result, role => role.AccountId == 10);
        Assert.Contains(result, role => role.AccountId == 11);
    }

    [Fact]
    public async Task GetRolesByContactAndAccountIdsAsync_WithNoMatchingRoles_ShouldReturnEmptyList()
    {
        // Arrange
        const int contactId = 456;
        var accountIds = new List<int> { 99 };

        var context = CreateSqliteContext();
        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.GetRolesByContactAndAccountIdsAsync(contactId, accountIds);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAccountIdsWithRoleLabelAsync_WithExistingLabels_ShouldReturnAccountIds()
    {
        // Arrange
        const int contactId = 456;
        const int labeledAccountId = 10;
        const int unlabeledAccountId = 11;
        var accountIds = new List<int> { labeledAccountId, unlabeledAccountId };

        using var context = new AccountContext(_dbContextOptions);
        context.RoleLabelEntity.Add(new RoleLabelEntity
        {
            AccountId = labeledAccountId,
            ContactId = contactId,
            LabelId = 1
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.GetAccountIdsWithRoleLabelAsync(contactId, accountIds);

        // Assert
        Assert.Single(result);
        Assert.Contains(labeledAccountId, result);
    }

    [Fact]
    public async Task GetAccountIdsWithRoleLabelAsync_WithNoLabels_ShouldReturnEmptySet()
    {
        // Arrange
        const int contactId = 456;
        var accountIds = new List<int> { 10, 11 };

        var context = CreateSqliteContext();
        var rolesRepository = new RoleRepository(context);

        // Act
        var result = await rolesRepository.GetAccountIdsWithRoleLabelAsync(contactId, accountIds);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnNotFound()
    {
        // Arrange
        var context = CreateSqliteContext();
        var rolesRepository = new RoleRepository(context);

        // Act
        Task RoleUpdate() => rolesRepository.UpdateRoleSignatoryAsync(1, 1, false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(RoleUpdate);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldDeleteRole()
    {
        // Arrange
        var context = CreateSqliteContext();
        var accountMock = _fixture.Create<AccountEntity>();
        var contactMock = _fixture.Build<ContactEntity>()
            .With(x => x.IsActive, true)
            .CreateMany();
        context.AccountEntity.Add(accountMock);
        context.ContactEntity.AddRange(contactMock);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

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

    [Theory]
    [InlineData(null!, "notfound@test.fr")]
    [InlineData(1, null!)]
    public async Task CheckRoleExistsAsync_ShouldReturnFalse_IfContactDoesNotExist(int? contactId, string? email)
    {
        // Arrange: Initialize the context
        var context = CreateSqliteContext();
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act: Call the CheckRoleExistsAsync method with the defined inputs
        var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(2, contactId, null, email);

        // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
        Assert.False(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ShouldReturnTrue_IfContactHasRoleOnSpecifiedAccount()
    {
        // Arrange: Initialize the context
        var context = CreateSqliteContext();
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act: Call the CheckRoleExistsAsync method with the defined inputs
        var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(firstCustomer.ContactId, secondCustomer.ContactId, accountId, secondCustomer.Email);

        // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ShouldReturnTrue_IfContactHasRoleOnAccountOfPrimaryContact()
    {
        // Arrange: Initialize the context
        var context = CreateSqliteContext();
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act: Call the CheckRoleExistsAsync method with the defined inputs
        var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(firstCustomer.ContactId, secondCustomer.ContactId, null, secondCustomer.Email);

        // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ShouldReturnFalse_IfContactHasRoleOnDifferentAccount()
    {
        // Arrange: Initialize the context
        var context = CreateSqliteContext();
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act: Call the CheckRoleExistsAsync method with the defined inputs
        var contactHasRoleOnAccount = await rolesRepository.CheckRoleExistsAsync(firstCustomer.ContactId, secondCustomer.ContactId, accountId, secondCustomer.Email);

        // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
        Assert.False(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task IsContactHasRoleInAccount_ShouldReturnTrue_IfContactHaveRole()
    {
        // Arrange: Initialize the context
        using var context = new AccountContext(_dbContextOptions);
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act: Call the CheckRoleExistsAsync method with the defined inputs
        var contactHasRoleOnAccount = await rolesRepository.IsContactHasRoleOnAccount(contactEntity.ContactId, 1, accountEntity.AccountNumber);

        // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task IsContactHasRoleInAccount_ShouldReturnFalse_IfContactHaventRole()
    {
        // Arrange: Initialize the context
        var context = CreateSqliteContext();
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
        context.ChangeTracker.Clear();

        var rolesRepository = new RoleRepository(context);

        // Act: Call the CheckRoleExistsAsync method with the defined inputs
        var contactHasRoleOnAccount = await rolesRepository.IsContactHasRoleOnAccount(contactEntity.ContactId, null, "wrongAccountNumber");

        // Assert: Verify if the contact passed as a parameter has a role on the account of the primary contact
        Assert.False(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task GetContactRolesAsync_WithProspectAccount_ShouldExcludeProspectAccount()
    {
        using var context = new AccountContext(_dbContextOptions);

        var contact = new ContactEntity
        {
            ContactId = 1,
            Type = ContactType.Collaborator.ToString(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean.dupont@test.fr",
            PersonaName = "Jean Dupont",
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
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var prospectAccount = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-PROSPECT-002",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.RoleEntity.AddRange(
            new RoleEntity { Account = clientAccount, Contact = contact, ContactId = contact.ContactId },
            new RoleEntity { Account = prospectAccount, Contact = contact, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        var result = await repository.GetContactRolesAsync(contact.ContactId, new Pagination { PageNumber = 1, PageSize = 10 });

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(clientAccount.AccountId, result.Items.Single().AccountId);
    }

    [Fact]
    public async Task GetSignatoryAsync_WithProspectAccount_ShouldThrowNotFoundException()
    {
        var context = CreateSqliteContext();

        var contact = new ContactEntity
        {
            ContactId = 1,
            Type = ContactType.Collaborator.ToString(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean.dupont@test.fr",
            PersonaName = "Jean Dupont",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
        var prospectAccount = new AccountEntity
        {
            AccountId = 3,
            AccountNumber = "ACC-PROSPECT-003",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.RoleEntity.Add(new RoleEntity
        {
            Account = prospectAccount,
            Contact = contact,
            ContactId = contact.ContactId,
            IsSignatory = true
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        await Assert.ThrowsAsync<NotFoundException>(() => repository.GetSignatoryAsync(prospectAccount.AccountId));
    }

    [Fact]
    public async Task CheckRoleExistsAsync_WithOnlyProspectSharedAccount_ShouldReturnFalse()
    {
        var context = CreateSqliteContext();

        var currentUser = new ContactEntity
        {
            ContactId = 10,
            Type = ContactType.Collaborator.ToString(),
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean.dupont@test.fr",
            PersonaName = "Jean Dupont",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
        var targetContact = new ContactEntity
        {
            ContactId = 20,
            Type = ContactType.Collaborator.ToString(),
            FirstName = "Paul",
            LastName = "Martin",
            Email = "paul.martin@test.fr",
            PersonaName = "Paul Martin",
            CreationDate = DateTime.UtcNow,
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
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.RoleEntity.AddRange(
            new RoleEntity { Account = prospectAccount, Contact = currentUser, ContactId = currentUser.ContactId },
            new RoleEntity { Account = prospectAccount, Contact = targetContact, ContactId = targetContact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        var result = await repository.CheckRoleExistsAsync(currentUser.ContactId, targetContact.ContactId, prospectAccount.AccountId, targetContact.Email);

        Assert.False(result);
    }

    [Fact]
    public async Task IsProspectAccountAsync_WhenAccountIsProspectAndActive_ShouldReturnTrue()
    {
        var context = CreateSqliteContext();

        var prospectAccount = new AccountEntity
        {
            AccountId = 50,
            AccountNumber = "ACC-PROSPECT-050",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true
        };
        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        var result = await repository.IsProspectAccountAsync(50);

        Assert.True(result);
    }

    [Fact]
    public async Task IsProspectAccountAsync_WhenAccountIsNotProspect_ShouldReturnFalse()
    {
        var context = CreateSqliteContext();

        var clientAccount = new AccountEntity
        {
            AccountId = 51,
            AccountNumber = "ACC-CLIENT-051",
            LegalName = "Client Account",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true
        };
        context.AccountEntity.Add(clientAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        var result = await repository.IsProspectAccountAsync(51);

        Assert.False(result);
    }

    [Fact]
    public async Task IsProspectAccountAsync_WhenAccountDoesNotExist_ShouldReturnFalse()
    {
        var context = CreateSqliteContext();

        var repository = new RoleRepository(context);

        var result = await repository.IsProspectAccountAsync(9999);

        Assert.False(result);
    }

    [Fact]
    public async Task IsContactHasRoleOnAccount_WithProspectAccount_ByAccountNumber_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 201,
            AccountNumber = "ACC-PROSPECT-201",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var contact = new ContactEntity
        {
            ContactId = 201,
            FirstName = "Jean",
            LastName = "Dupont",
            Email = "jean.dupont@prospect.fr",
            PersonaName = "Jean Dupont",
            Type = "Collaborateur",
            IsActive = true
        };

        context.RoleEntity.Add(new RoleEntity { Account = prospectAccount, Contact = contact, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        var result = await repository.IsContactHasRoleOnAccount(contact.ContactId, null, prospectAccount.AccountNumber);

        Assert.True(result);
    }

    [Fact]
    public async Task IsContactHasRoleOnAccount_WithProspectAccount_ByAccountId_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 202,
            AccountNumber = "ACC-PROSPECT-202",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var contact = new ContactEntity
        {
            ContactId = 202,
            FirstName = "Marie",
            LastName = "Dupont",
            Email = "marie.dupont@prospect.fr",
            PersonaName = "Marie Dupont",
            Type = "Collaborateur",
            IsActive = true
        };

        context.RoleEntity.Add(new RoleEntity { Account = prospectAccount, Contact = contact, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        var result = await repository.IsContactHasRoleOnAccount(contact.ContactId, prospectAccount.AccountId, null);

        Assert.True(result);
    }

    [Fact]
    public async Task GetAccountsWhereContactIsLastCollaboratorAsync_Should_ReturnAccounts_WhereContactIsOnlyCollaborator()
    {
        // Arrange
        var contactId = 25;
        var context = CreateSqliteContext();

        SeedContact(context, contactId, ContactType.Collaborator);
        SeedContact(context, 26, ContactType.Collaborator);
        SeedContact(context, 99, ContactType.Customer);

        SeedRole(context, accountId: 10, contactId);                  // seul collaborateur -> dernier
        SeedRole(context, accountId: 20, contactId);                  // 2 collaborateurs   -> pas dernier
        SeedRole(context, accountId: 20, contactId: 26);
        SeedRole(context, accountId: 30, contactId);                  // seul collaborateur + 1 client -> dernier
        SeedRole(context, accountId: 30, contactId: 99);
        SeedRole(context, accountId: 40, contactId: 26);              // 25 n'a pas de rôle -> exclu

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        // Act
        var result = await repository.GetAccountsWhereContactIsLastCollaboratorAsync(contactId, new[] { 10, 20, 30, 40 });

        // Assert
        Assert.Equal(new[] { 10, 30 }, result.OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task GetAccountsWhereContactIsLastCollaboratorAsync_Should_ReturnEmpty_WhenAccountHasOtherCollaborators()
    {
        // Arrange
        var contactId = 25;
        var context = CreateSqliteContext();

        SeedContact(context, contactId, ContactType.Collaborator);
        SeedContact(context, 26, ContactType.Collaborator);

        SeedRole(context, accountId: 20, contactId);
        SeedRole(context, accountId: 20, contactId: 26);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        // Act
        var result = await repository.GetAccountsWhereContactIsLastCollaboratorAsync(contactId, new[] { 20 });

        // Assert
        Assert.Empty(result);
    }

    private void SeedContact(AccountContext context, int contactId, ContactType type)
    {
        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, contactId)
            .With(c => c.Type, type.ToString())
            .With(c => c.IsActive, true)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Create();
        context.ContactEntity.Add(contact);
    }

    private static void SeedRole(AccountContext context, int accountId, int contactId)
    {
        context.RoleEntity.Add(new RoleEntity { AccountId = accountId, ContactId = contactId });
    }

    [Fact]
    public async Task UpdateLastActivityDateAsync_WhenContactIsCollaboratorAndRoleExists_ShouldUpdateLastActivityDate()
    {
        // Arrange
        const int accountId = 300;
        const int contactId = 400;
        var expectedDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        var context = CreateSqliteContext();
        context.RoleEntity.Add(new RoleEntity { AccountId = accountId, ContactId = contactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        // Act
        await repository.UpdateLastActivityDateAsync(accountId, contactId, ContactType.Collaborator.ToString(), expectedDate);

        // Assert
        var updatedRole = await context.RoleEntity.FirstAsync(r => r.AccountId == accountId && r.ContactId == contactId);
        Assert.Equal(expectedDate, updatedRole.LastActivityDate);
    }

    [Theory]
    [InlineData("Customer")]
    [InlineData("Prospect")]
    [InlineData("")]
    public async Task UpdateLastActivityDateAsync_WhenContactTypeIsNotCollaborator_ShouldNotUpdateRole(string contactType)
    {
        // Arrange
        const int accountId = 301;
        const int contactId = 401;
        var originalDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var context = CreateSqliteContext();
        context.RoleEntity.Add(new RoleEntity { AccountId = accountId, ContactId = contactId, LastActivityDate = originalDate });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        // Act
        await repository.UpdateLastActivityDateAsync(accountId, contactId, contactType, DateTime.UtcNow);

        // Assert
        var role = await context.RoleEntity.FirstAsync(r => r.AccountId == accountId && r.ContactId == contactId);
        Assert.Equal(originalDate, role.LastActivityDate);
    }

    [Fact]
    public async Task UpdateLastActivityDateAsync_WhenCalledMultipleTimes_ShouldOverwriteWithLatestDate()
    {
        // Arrange
        const int accountId = 303;
        const int contactId = 403;
        var firstDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var secondDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var context = CreateSqliteContext();
        context.RoleEntity.Add(new RoleEntity { AccountId = accountId, ContactId = contactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RoleRepository(context);

        // Act
        await repository.UpdateLastActivityDateAsync(accountId, contactId, ContactType.Collaborator.ToString(), firstDate);
        await repository.UpdateLastActivityDateAsync(accountId, contactId, ContactType.Collaborator.ToString(), secondDate);

        // Assert
        var updatedRole = await context.RoleEntity.FirstAsync(r => r.AccountId == accountId && r.ContactId == contactId);
        Assert.Equal(secondDate, updatedRole.LastActivityDate);
    }

    private static AccountContext CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        connection.CreateFunction("newid", () => Guid.NewGuid().ToString());
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AccountContext(options);
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
        return context;
    }
}
