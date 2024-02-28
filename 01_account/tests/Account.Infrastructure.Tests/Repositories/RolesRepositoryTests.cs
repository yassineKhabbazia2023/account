// <copyright file="RolesRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            var accountsEntity = _fixture.Create<List<TAccount>>();
            context.TAccount.AddRange(accountsEntity);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);
            var contactId = accountsEntity.First().TRole.First().ContactId;

            var accountObjects = accountsEntity
                                    .SelectMany(item => item.TRole)
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
            var accountsMock = _fixture.Create<List<TAccount>>();
            context.TAccount.AddRange(accountsMock);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);
            var data = accountsMock.First().TRole.Where(r => r.IsSignatory!.Value).ToList();
            var resultExpected = new List<Contact>();
            resultExpected.AddRange(data.MapToContacts());

            // Act
            var roles = await rolesRepository.GetSignatoryAsync(data.First().AccountId);

            // Assert
            Assert.Equivalent(resultExpected, roles);
        }
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

        accountContext.TAccount.Add(new TAccount
        {
            AccountId = accountId,
            AccountNumber = "00001114455",
            CreatedBy = "UnitTest@kpmg.fr",
            Email = "account-mail@kpmg.fr",
            LegalName = "Pulse",
            SourceAccountNumber = "IBS",
        });
        accountContext.TContact.Add(new TContact
        {
            ContactId = contactId,
            ContactEmail = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "Customer",
        });

        await accountContext.SaveChangesAsync();

        var rolesRepository = new RoleRepository(accountContext);

        // Act
        await rolesRepository.CreateRoleAsync(roleRequest);

        // Assert
        Assert.Single(accountContext.TRole);
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
        accountContext.TContact.Add(new TContact
        {
            ContactId = contactId,
            ContactEmail = "Contact-mail@kpmg.fr",
            FirstName = "Contact-FN",
            LastName = "Contact-LT",
            Type = "Customer",
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

        accountContext.TAccount.Add(new TAccount
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
            var roleMock = _fixture.Create<TRole>();
            roleMock.IsSignatory = true;
            context.TRole.Add(roleMock);
            context.SaveChanges();

            var rolesRepository = new RoleRepository(context);

            // Act
            await rolesRepository.UpdateRoleSignatoryAsync(roleMock.AccountId, roleMock.ContactId, false);
            var roleObjects = context.TRole
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
}
