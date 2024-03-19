// <copyright file="RolesRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
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
            var accountsEntity = _fixture.Create<List<AccountEntity>>();
            context.AccountEntity.AddRange(accountsEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);
            var contactId = accountsEntity.First().RoleEntity.First().ContactId;

            var accountObjects = accountsEntity
                                    .SelectMany(item => item.RoleEntity)
                                    .Where(x => x.ContactId == contactId)
                                    .Select(x => x.Account.MapToAccount(contactId));

            Paging<AccountModel> accountPaging = new Paging<AccountModel>()
            {
                CurrentPage = 1,
                Items = accountObjects,
                TotalItems = accountObjects.Count(),
                TotalPage = 1
            };

            // Act
            var accounts = await rolesRepository.GetContactRolesAsync(contactId, pageNumber: 1, pageSize: 4);

            // Assert
            var accountExpect = JsonConvert.SerializeObject(accountPaging);
            var accountReceived = JsonConvert.SerializeObject(accounts);
            Assert.Equal(accountExpect, accountReceived);
        }
    }

    [Fact]
    public async Task GetSignatoryAsync_ShouldReturnCorrect()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountsMock = _fixture.Create<List<AccountEntity>>();
            context.AccountEntity.AddRange(accountsMock);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);
            var data = accountsMock.First().RoleEntity.Where(r => r.IsSignatory!.Value).ToList();
            var resultExpected = new List<Contact>();
            resultExpected.AddRange(data.MapToContacts());

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

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetSignatoryAsync(It.IsAny<int>()));

        Assert.Equal(Errors.NotFoundAccountCode, result.Code);
        Assert.Equal(Errors.NotFoundAccountMessage, result.Message);
    }

    [Fact]
    public async Task CreateRoleAsync_WithValidRequest_ShouldReturnCreated()
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

        using var accountContext = new AccountContext(_dbContextOptions);

        accountContext.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            SourceAccountNumber = "IBS",
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
            CreationDate = DateTime.UtcNow,
        });

        await accountContext.SaveChangesAsync();

        var rolesRepository = new RoleRepository(accountContext);

        // Act
        await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        Assert.Single(accountContext.RoleEntity);
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
    public async Task CreateRoleAsync_WithInValidContact_ShouldReturnCreated()
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

        using var accountContext = new AccountContext(_dbContextOptions);

        accountContext.AccountEntity.Add(new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            SourceAccountNumber = "IBS",
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
            Assert.Equal(false, role.IsSignatory);
        }
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
            var contactMock = _fixture.Create<List<ContactEntity>>();
            await InitRoleMockData(context, accountMock, contactMock);
            var accountRepository = new AccountRepository(context);
            var roleRepository = new RoleRepository(context);
            var rolesBefore = await accountRepository.GetContactsAccountAsync(accountMock.AccountId);

            // Act
            await roleRepository.DeleteRoleAsync(accountMock.AccountId, contactMock.First().ContactId);

            // Assert
            var roles = await accountRepository.GetContactsAccountAsync(accountMock.AccountId);
            Assert.Equal(rolesBefore.Count() - 1, roles.Count());
        }
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldDeleteRole_CasTwoSignatory()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountMock = _fixture.Create<AccountEntity>();
            var contactMock = _fixture.Create<List<ContactEntity>>();
            await InitRoleMockData(context, accountMock, contactMock);
            var accountRepository = new AccountRepository(context);
            var roleRepository = new RoleRepository(context);
            var rolesBefore = await accountRepository.GetContactsAccountAsync(accountMock.AccountId);

            // Act
            await roleRepository.DeleteRoleAsync(accountMock.AccountId, contactMock.First().ContactId);

            // Assert
            var roles = await accountRepository.GetContactsAccountAsync(accountMock.AccountId);
            Assert.Equal(rolesBefore.Count() - 1, roles.Count());
        }
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrowException_CaseNotFoundContact()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountMock = _fixture.Create<AccountEntity>();
            var contactMock = _fixture.Create<List<ContactEntity>>();
            await InitRoleMockData(context, accountMock, contactMock);
            var roleRepository = new RoleRepository(context);

            // Act
            Task DeleteRole() => roleRepository!.DeleteRoleAsync(accountMock!.AccountId, It.IsAny<int>());

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(DeleteRole);
        }
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrowException_CaseOnlyOneSignatory()
    {
        // Arrange
        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountMock = _fixture.Create<AccountEntity>();
            var contactMock = _fixture.CreateMany<ContactEntity>(2);
            await InitRoleMockData(context, accountMock, contactMock);
            context.RoleEntity.RemoveRange(context.RoleEntity.AsEnumerable());
            context.SaveChanges();
            var roleRepository = new RoleRepository(context);

            // Act
            Task DeleteRole() => roleRepository!.DeleteRoleAsync(accountMock!.AccountId, contactMock.Last().ContactId);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
        }
    }

    private static async Task InitRoleMockData(AccountContext context, AccountEntity accountMock, IEnumerable<ContactEntity> contactMock)
    {
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
    }
}
