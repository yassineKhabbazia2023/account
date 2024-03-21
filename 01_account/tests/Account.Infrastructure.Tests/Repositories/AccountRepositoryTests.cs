// <copyright file="AccountRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq;
using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;
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
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                accountsModel.First().AddressEntity.First().AddressType = AddressType.delivery.ToString();
                context.AccountEntity.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);
                var contactId = accountsModel.Select(account => account.RoleEntity.Select(role => role.ContactId).FirstOrDefault()).FirstOrDefault();
                var accountObject = accountsModel.Select(item => item.MapToAccount(contactId)) ?? Enumerable.Empty<AccountModel>();
                Paging<AccountModel> accountPaging = new Paging<AccountModel>()
                {
                    CurrentPage = 1,
                    Items = accountObject!,
                    TotalItems = accountObject.Count(),
                    TotalPage = 1
                };

                // Act
                var search = accountObject.First()!.LegalName;
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
                    Items = Enumerable.Empty<AccountModel>()!,
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
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                var accountFirst = accountsModel[0];
                var accountDetail = accountFirst?.MapToAccountDetail();
                context.AccountEntity.AddRange(accountsModel);
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
                var accountRepository = new AccountRepository(context);

                // Act
                Task Accounts() => accountRepository.GetAccountDetailAsync(123);

                // Assert
                await Assert.ThrowsAsync<NotFoundException>(Accounts);
            }
        }

        [Fact]
        public async Task GetAccountAsync_Should_ReturnsAccountAsync()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                var accountFirst = accountsModel[0];
                var accountDetail = accountFirst?.MapToAccountDetail();
                context.AccountEntity.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);

                // Act
                var accounts = await accountRepository.GetAccountAsync(accountFirst!.AccountId);

                // Assert
                Assert.Equal(accountDetail?.AccountNumber, accounts!.AccountNumber);
                Assert.Equal(accountDetail?.AccountId, accounts.AccountId);
                Assert.Equal(accountDetail?.Legal?.LegalName, accounts.Legal?.LegalName);
                Assert.Equal(accountDetail?.Legal?.Siren, accounts.Legal?.Siren);
                Assert.Equal(accountDetail?.Legal?.Siret, accounts.Legal?.Siret);
            }
        }

        [Fact]
        public async Task GetAccountAsync_Should_ThrowsNotFoundExceptionAsync()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountRepository = new AccountRepository(context);

                // Act
                Task Accounts() => accountRepository.GetAccountAsync(123);

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
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                var accountFirst = accountsModel[0];
                var accountDetail = accountFirst?.MapToAccountDetail();
                if (accountDetail?.Accounting != null)
                {
                    accountDetail.Accounting.TaxationSystem = "Impot sur le revenu";
                }

                context.AccountEntity.AddRange(accountsModel);
                await context.SaveChangesAsync();
                var accountRepository = new AccountRepository(context);

                // Act
                await accountRepository.UpdateAccountAsync(accountDetail!.AccountId, accountDetail!);

                // Assert
                var updatedAccont = await context.AccountEntity.SingleAsync(a => a.AccountId == accountDetail.AccountId);
                Assert.Equal(accountDetail?.AccountNumber, updatedAccont!.AccountNumber);
                Assert.Equal(accountDetail?.AccountId, updatedAccont.AccountId);
                Assert.Equal(accountDetail?.Accounting?.TaxationSystem, updatedAccont.TaxationSystem);
                Assert.Equal(accountDetail?.Accounting?.ActivityType, updatedAccont.ActivityType);
                Assert.Equal(accountDetail?.Accounting?.ActivityDescription, updatedAccont.ActivityDescription);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(ContactType.collaborator)]
        [InlineData(ContactType.customer)]
        public async Task GetContactsAccountAsync_WhenAccountIdIsValid_ShouldReturnContactsAccount(ContactType? type)
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);

            _fixture.Customizations.Add(new ContactEntityTypeSpecimenBuilder());
            var resultExpected = new List<Contact>();

            var accountMock = _fixture.Build<AccountEntity>()
                                           .Without(a => a.DelegationEntity)
                                           .Without(a => a.RoleEntity)
                                           .Create();

            for (int i = 0; i < 20; i++)
            {
                var contactMock = _fixture.Build<ContactEntity>()
                                           .Without(c => c.DelegationEntityDelegatee)
                                           .Without(c => c.DelegationEntityDelegator)
                                           .Without(c => c.RoleEntity)
                                           .Without(c => c.ContactGlobalUniqueId)
                                           .Create();

                var roleMock = _fixture.Build<RoleEntity>()
                                       .With(e => e.ContactId, contactMock.ContactId)
                                       .With(e => e.Contact, contactMock)
                                       .With(e => e.AccountId, accountMock.AccountId)
                                       .With(e => e.Account, accountMock)
                                       .Create();

                context.RoleEntity.Add(roleMock);
                context.SaveChanges();

                if (type == null || contactMock.Type == type.ToString())
                {
                    resultExpected.Add(contactMock.MapToContact() !);
                }
            }

            var accountRepository = new AccountRepository(context);

            // Act
            var roles = await accountRepository.GetContactsAccountAsync(accountMock.AccountId, type);

            // Assert
            Assert.Equivalent(resultExpected, roles);
        }

        [Fact]
        public async Task GetContactsAccountByAdminAsync_ShouldReturnsContacts()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var resultExpected = new List<Contact>();
                var accountsMock = _fixture.Create<List<AccountEntity>>();
                var contactAdmin = _fixture.Build<ContactEntity>()
                                           .Without(c => c.DelegationEntityDelegatee)
                                           .Without(c => c.DelegationEntityDelegator)
                                           .Without(c => c.RoleEntity)
                                           .Without(c => c.ContactGlobalUniqueId)
                                           .Create();

                context.AccountEntity.AddRange(accountsMock);
                context.SaveChanges();

                for (int i = 0; i < accountsMock.Count; i++)
                {
                    var roleMock = new RoleEntity()
                    {
                        ContactId = contactAdmin.ContactId,
                        Contact = contactAdmin,
                        AccountId = accountsMock[i].AccountId,
                        Account = accountsMock[i]
                    };

                    context.RoleEntity.Add(roleMock);
                    context.SaveChanges();

                    resultExpected.AddRange(accountsMock[i].RoleEntity.Select(x => x.Contact.ToContact() !));
                }

                var accountRepository = new AccountRepository(context);

                // Act
                var contactByAdmin = await accountRepository.GetContactsAccountByAdminAsync(contactAdmin.ContactId);
                var expected = resultExpected.Select(x => x.ContactId).Distinct();

                // Assert
                Assert.Equivalent(expected, contactByAdmin.Select(x => x.ContactId));
            }
        }
    }
}
