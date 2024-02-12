// <copyright file="AccountRepositoryTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Infrastructure.Tests.Configuration;
using Newtonsoft.Json;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using AccountModel = Pulse.Account.Core.Models.Account;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class AccountRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _options;

        public AccountRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Should_GetAccountList_ReturnsOkResultAsync()
        {
            // Arrange
            var accountsModel = _fixture.Create<List<TAccount>>();
            var accountRepository = UnitTestUtils.InitAccountRepository(_fixture, accountsModel);
            var accountObject = accountsModel.Select(item => item.TAccountToAccountModel(123));
            Paging<AccountModel> accountPaging = new Paging<AccountModel>()
            {
                CurrentPage = 1,
                Items = accountObject,
                TotalItems = accountObject.Count(),
                TotalPage = 1
            };

            // Act
            var accounts = await accountRepository.GetAccountsAsync(search: string.Empty, contactId: 123, page: 1, limit: 4);

            // Assert
            var accountExpect = JsonConvert.SerializeObject(accountPaging);
            var accountReceived = JsonConvert.SerializeObject(accounts);
            Assert.Equal(accountExpect, accountReceived);
        }

        [Fact]
        public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
        {
            // Arrange
            var accountsModel = _fixture.Create<List<TAccount>>();
            var accountFirst = accountsModel.FirstOrDefault();
            var accountDetail = accountFirst?.TAccountToAccountDetail();
            var accountRepository = UnitTestUtils.InitAccountRepository(_fixture, accountsModel);

            // Act
            var accounts = await accountRepository.GetAccountDetailAsync(accountFirst!.AccountId);

            // Assert
            Assert.Equal(accountDetail?.AccountNumber, accounts.AccountNumber);
            Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
            Assert.Equal(accountDetail?.Legal?.LegalName, accounts.Legal?.LegalName);
        }

        [Fact]
        public async Task Should_Statistics_Nominal()
        {
            using (var context = new AccountContext(_options))
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
                var roles = new List<TRoles>
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
                context.TRoles.AddRange(roles);
                await context.SaveChangesAsync();

                var result = await repository.GetStatisticsAsync(1);

                Assert.NotNull(result);
                Assert.Equal(0, result.AccountToDeploy);
                Assert.Equal(1, result.AccountConnected);
                Assert.Equal(2, result.AccountInProgress);
            }
        }
    }
}
