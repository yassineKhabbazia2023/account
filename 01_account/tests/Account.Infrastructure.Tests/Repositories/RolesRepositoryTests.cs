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
    public void CreateRoleAsync_ShouldReturnCreated()
    {
        // Arrange
        using (var context = new AccountContext(_context))
        {
            var roleMock = _fixture.Create<CreateRole>();

            var rolesRepository = new RoleRepository(context);

            // Act
            var result = rolesRepository.CreateRoleAsync(roleMock);

            // Assert
            Assert.Equal(Task.CompletedTask, result);
        }
    }
}
