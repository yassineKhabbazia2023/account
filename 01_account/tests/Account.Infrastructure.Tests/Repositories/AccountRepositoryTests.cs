// <copyright file="AccountRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Collections.Generic;
using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class AccountRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _dbContextOptions;

        public AccountRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAccountList_Should_ReturnsOkResultAsync()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<TAccount>>();
                context.TAccount.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);
                var contactId = accountsModel.Select(account => account.TRole.Select(role => role.ContactId).FirstOrDefault()).FirstOrDefault();
                var accountObject = accountsModel.Select(item => item.MapToAccount(contactId)) ?? Enumerable.Empty<AccountModel>();
                Paging<AccountModel> accountPaging = new Paging<AccountModel>()
                {
                    CurrentPage = 1,
                    Items = accountObject!,
                    TotalItems = accountObject.Count(),
                    TotalPage = 1
                };

                // Act
                var search = accountObject.First() !.LegalName;
                var accounts = await accountRepository.GetAccountsAsync(search: search, pageNumber: 1, pageSize: 4, contactId);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
                var accountReceived = JsonConvert.SerializeObject(accounts.Items?.FirstOrDefault());
                Assert.Contains(accountReceived, accountExpect);
            }
        }

        [Fact]
        public async Task GetAccountList_When_No_Rows_Found_Should_Return_Empty_List()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountRepository = new AccountRepository(context);
                var accountPaging = new Paging<AccountModel>()
                {
                    CurrentPage = 1,
                    Items = Enumerable.Empty<AccountModel>() !,
                    TotalItems = 0,
                    TotalPage = 1
                };

                // Act
                var accounts = await accountRepository.GetAccountsAsync(search: string.Empty, pageNumber: 1, pageSize: 4, contactId: 100);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
                var accountReceived = JsonConvert.SerializeObject(accounts.Items);
                Assert.Contains(accountReceived, accountExpect);
            }
        }

        [Fact]
        public async Task GetAccountDetail_Should_ReturnsOkResultAsync()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<TAccount>>();
                var accountFirst = accountsModel[0];
                var accountDetail = accountFirst?.MapToAccountDetail();
                context.TAccount.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);

                // Act
                var accounts = await accountRepository.GetAccountDetailAsync(accountFirst!.AccountId);

                // Assert
                Assert.Equal(accountDetail?.AccountNumber, accounts!.AccountNumber);
                Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
                Assert.Equal(accountDetail?.Legal?.LegalName, accounts.Legal?.LegalName);
                Assert.Equal(accountDetail?.Legal?.Siren, accounts.Legal?.Siren);
                Assert.Equal(accountDetail?.Legal?.Siret, accounts.Legal?.Siret);
            }
        }

        [Fact]
        public async Task GetAccountDetail_Should_ReturnsNotFoundResultAsync()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<TAccount>>();
                context.TAccount.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);

                // Act
                Task Accounts() => accountRepository.GetAccountDetailAsync(123);

                // Assert
                await Assert.ThrowsAsync<NotFoundException>(Accounts);
            }
        }

        [Fact]
        public async Task UpdateAccount_Should_ReturnsOkResultAsync()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<TAccount>>();
                var accountFirst = accountsModel[0];
                var accountDetail = accountFirst?.MapToAccountDetail();
                if (accountDetail?.Accounting != null)
                {
                    accountDetail.Accounting.TaxationSystem = "Impot sur le revenu";
                }

                context.TAccount.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);

                // Act
                var accounts = await accountRepository.UpdateAccountAsync(accountDetail!, accountDetail!.AccountId);

                // Assert
                Assert.Equal(accountDetail?.AccountNumber, accounts!.AccountNumber);
                Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
                Assert.Equal(accountDetail?.Accounting?.TaxationSystem, accounts.Accounting?.TaxationSystem);
                Assert.Equal(accountDetail?.Accounting?.ActivityType, accounts.Accounting?.ActivityType);
                Assert.Equal(accountDetail?.Accounting?.ActivityDescription, accounts.Accounting?.ActivityDescription);
            }
        }

        [Fact]
        public async Task Should_Statistics_Nominal()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                var expected = new List<TDeploymentPlanning>
                {
                    new()
                    {
                        AccountId = 1,
                        Status = 1
                    },
                    new()
                    {
                        AccountId = 2,
                        Status = 1
                    },
                    new()
                    {
                        AccountId = 1,
                        Status = 2
                    },
                    new()
                    {
                        AccountId = 3,
                        Status = 0,
                    }
                };
                var roles = new List<TRole>
                {
                    new()
                    {
                        ContactId = 1,
                        AccountId = 1
                    },
                    new()
                    {
                        ContactId = 1,
                        AccountId = 2
                    },
                    new()
                    {
                        ContactId = 2,
                        AccountId = 3
                    }
                };
                var repository = new AccountRepository(context);
                context.TDeploymentPlanning.AddRange(expected);
                context.TRole.AddRange(roles);
                await context.SaveChangesAsync();

                var result = await repository.GetStatisticsAsync(1);

                Assert.NotNull(result);
                Assert.Equal(0, result.AccountToDeploy);
                Assert.Equal(1, result.AccountConnected);
                Assert.Equal(2, result.AccountInProgress);
            }
        }

        [Fact]
        public async Task GetContactsAccountAsync_Should_Return_ContactsAccount()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var accountsMock = _fixture.Create<List<TAccount>>();
            context.TAccount.AddRange(accountsMock);
            context.SaveChanges();

            var accountRepository = new AccountRepository(context);
            var data = accountsMock.First().TRole.ToList();
            var resultExpected = new List<Contact>();
            resultExpected.AddRange(data.MapToContacts());

            // Act
            var roles = await accountRepository.GetContactsAccountAsync(data.First().AccountId);

            // Assert
            Assert.Equivalent(resultExpected, roles);
        }
    }
}
