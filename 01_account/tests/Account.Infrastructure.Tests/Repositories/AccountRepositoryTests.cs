// <copyright file="AccountRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
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

        [Theory]
        [InlineData("199900046522")]
        [InlineData("test scA")]
        [InlineData("firstuser")]
        [InlineData("lastuser")]
        [InlineData("firstlastuser@test.fr")]
        public async Task GetAccountListSearch_Should_ReturnsOkResultAsync(string criteria)
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                accountsModel.First().AddressEntity.First().AddressType = AddressType.delivery.ToString();
                accountsModel.First().AccountNumber = "199900046522";
                accountsModel.First().LegalName = "Test SCA";
                accountsModel.First().RoleEntity.First().Contact.FirstName = "FirstUser";
                accountsModel.First().RoleEntity.First().Contact.LastName = "LastUser";
                accountsModel.First().RoleEntity.First().Contact.Email = "firstLastUser@test.fr";
                accountsModel.First().Hub.HubId = 1;
                accountsModel.First().Hub.HubName = "Hubname";
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
                var searchAccountCriteria = new SearchAccountCriteria
                {
                    Search = criteria,
                    ContactId = contactId
                };
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 4
                };

                // Act
                var accounts = await accountRepository.GetAccountsAsync(searchAccountCriteria, pagination);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
                var accountReceived = JsonConvert.SerializeObject(accounts.Items?.First());
                Assert.Contains(accountReceived, accountExpect);
            }
        }

        [Fact]
        public async Task GetAccountListSearchStatus_Should_ReturnsOkResultAsync()
        {
            // Arrange
            using (var context = new AccountContext(_dbContextOptions))
            {
                var accountsModel = _fixture.Create<List<AccountEntity>>();
                accountsModel.First().AddressEntity.First().AddressType = AddressType.delivery.ToString();
                accountsModel.First().AccountNumber = "199900046522";
                accountsModel.First().DeploymentEntity.First().Status = 1;
                accountsModel.First().LegalName = "Test SCA";
                accountsModel.First().RoleEntity.First().Contact.FirstName = "FirstUser";
                accountsModel.First().RoleEntity.First().Contact.LastName = "LastUser";
                accountsModel.First().RoleEntity.First().Contact.Email = "firstLastUser@test.fr";
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
                Paging<AccountModel> accountPagingEmpty = new Paging<AccountModel>()
                {
                    CurrentPage = 1,
                    Items = new List<AccountModel>(),
                    TotalItems = 0,
                    TotalPage = 0
                };
                var searchAccountCriteria = new SearchAccountCriteria
                {
                    DeploymentStatus = (int)DeploymentStatus.ToDeploy,
                    ContactId = contactId
                };
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 4
                };

                // Act
                var accounts = await accountRepository.GetAccountsAsync(searchAccountCriteria, pagination);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
                var accountReceived = JsonConvert.SerializeObject(accounts.Items?.First());
                Assert.Contains(accountReceived, accountExpect);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]

        public async Task GetAccountList_Should_ReturnsOkResultAsync(int pageSize)
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var contactId = 123;
                var resultExpected = new List<AccountModel>();
                var contactMock = _fixture.Build<ContactEntity>()
                                               .Without(c => c.DelegationEntityDelegatee)
                                               .Without(c => c.DelegationEntityDelegator)
                                               .Without(c => c.RoleEntity)
                                               .Without(c => c.ContactGlobalUniqueId)
                                               .With(c => c.ContactId, contactId)
                                               .Create();

                for (int i = 0; i < 3; i++)
                {
                    var deploimentEntityMock = _fixture.Build<DeploymentEntity>()
                        .With(a => a.Status, 1)
                        .Create();
                    var accountMock = _fixture.Build<AccountEntity>()
                                                   .Without(a => a.Delegation)
                                                   .Without(a => a.RoleEntity)
                                                   .With(a => a.DeploymentEntity, new List<DeploymentEntity> { deploimentEntityMock })
                                                   .Create();

                    var roleMock = _fixture.Build<RoleEntity>()
                                           .With(e => e.ContactId, contactMock.ContactId)
                                           .With(e => e.Contact, contactMock)
                                           .With(e => e.AccountId, accountMock.AccountId)
                                           .With(e => e.Account, accountMock)
                                           .Create();

                    accountMock.RoleEntity.Add(roleMock);

                    context.AccountEntity.Add(accountMock);
                    context.SaveChanges();

                    resultExpected.Add(accountMock.MapToAccount(contactMock.ContactId)!);
                }

                var accountRepository = new AccountRepository(context);

                Paging<AccountModel> accountPaging = new Paging<AccountModel>()
                {
                    CurrentPage = 1,
                    Items = resultExpected!,
                    TotalItems = resultExpected.Count,
                    TotalPage = Paginator.GetTotalPages(resultExpected.Count, pageSize)
                };
                var searchAccountCriteria = new SearchAccountCriteria
                {
                    ContactId = contactId
                };
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = pageSize
                };

                // Act
                var accounts = await accountRepository.GetAccountsAsync(searchAccountCriteria, pagination);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
                var accountReceived = JsonConvert.SerializeObject(accounts.Items?.FirstOrDefault());
                Assert.Contains(accountReceived, accountExpect);
                Assert.Equal(accountPaging.TotalPage, accounts.TotalPage);
                Assert.Equal(accountPaging.TotalItems, accounts.TotalItems);
                Assert.Equal(accountPaging.CurrentPage, accounts.CurrentPage);
            }
        }

        [Fact]
        public async Task GetAccountList_ReturnOnlyAccountActive()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var contactId = 123;
                var resultExpected = new List<AccountModel>();
                var contactMock = _fixture.Build<ContactEntity>()
                                               .Without(c => c.DelegationEntityDelegatee)
                                               .Without(c => c.DelegationEntityDelegator)
                                               .Without(c => c.RoleEntity)
                                               .Without(c => c.ContactGlobalUniqueId)
                                               .With(c => c.ContactId, contactId)
                                               .Create();

                var deploymentMockActive = _fixture.Build<DeploymentEntity>()
                    .With(a => a.Status, 1)
                    .Create();

                var accountMock = _fixture.Build<AccountEntity>()
                                               .Without(a => a.Delegation)
                                               .Without(a => a.RoleEntity)
                                               .With(a => a.DeploymentEntity, new List<DeploymentEntity> { deploymentMockActive })
                                               .Create();

                var roleMock = _fixture.Build<RoleEntity>()
                                       .With(e => e.ContactId, contactMock.ContactId)
                                       .With(e => e.Contact, contactMock)
                                       .With(e => e.AccountId, accountMock.AccountId)
                                       .With(e => e.Account, accountMock)
                                       .Create();

                accountMock.RoleEntity.Add(roleMock);

                context.AccountEntity.Add(accountMock);

                resultExpected.Add(accountMock.MapToAccount(contactMock.ContactId)!);

                var deploymentMock = _fixture.Build<DeploymentEntity>()
                    .With(a => a.Status, 4)
                    .Create();

                var accountMockInactive = _fixture.Build<AccountEntity>()
                                                  .Without(a => a.Delegation)
                                                  .Without(a => a.RoleEntity)
                                                  .With(a => a.DeploymentEntity, new List<DeploymentEntity> { deploymentMock })
                                                  .Create();
                context.AccountEntity.Add(accountMockInactive);
                context.SaveChanges();

                var accountRepository = new AccountRepository(context);

                Paging<AccountModel> accountPaging = new Paging<AccountModel>()
                {
                    CurrentPage = 1,
                    Items = resultExpected!,
                    TotalItems = resultExpected.Count,
                    TotalPage = Paginator.GetTotalPages(resultExpected.Count, 1)
                };
                var searchAccountCriteria = new SearchAccountCriteria
                {
                    ContactId = contactId
                };
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 1
                };

                // Act
                var accounts = await accountRepository.GetAccountsAsync(searchAccountCriteria, pagination);

                // Assert
                var accountExpect = JsonConvert.SerializeObject(accountPaging.Items);
                var accountReceived = JsonConvert.SerializeObject(accounts.Items?.FirstOrDefault());
                Assert.Contains(accountReceived, accountExpect);
                Assert.Equal(accountPaging.TotalPage, accounts.TotalPage);
                Assert.Equal(accountPaging.TotalItems, accounts.TotalItems);
                Assert.Equal(accountPaging.CurrentPage, accounts.CurrentPage);
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
                var searchAccountCriteria = new SearchAccountCriteria
                {
                    ContactId = 100
                };
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 4
                };

                // Act
                var accounts = await accountRepository.GetAccountsAsync(searchAccountCriteria, pagination);

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
                Assert.Equal(accountDetail?.Legal?.CreationDate, accounts.Legal?.CreationDate);
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
        [InlineData(null, null, false)]
        [InlineData(null, "name", false)]
        [InlineData(null, "name", true)]
        [InlineData(ContactType.Collaborator, null, false)]
        [InlineData(ContactType.Customer, null, false)]
        [InlineData(ContactType.Collaborator, "name", false)]
        [InlineData(ContactType.Customer, "name", true)]
        public async Task GetContactsAccountAsync_WhenAccountIdIsValid_ShouldReturnContactsAccount(ContactType? type, string? sorting, bool descending)
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);

            _fixture.Customizations.Add(new ContactEntityTypeSpecimenBuilder());
            var resultExpected = new List<Contact>();

            var accountMock = _fixture.Build<AccountEntity>()
                                           .Without(a => a.Delegation)
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

                if (type == null || contactMock.Type == type.ToString()!.ToLower())
                {
                    contactMock.Type = type.ToString();
                    resultExpected.Add(contactMock.MapToContact()!);
                }

                context.RoleEntity.Add(roleMock);
                context.SaveChanges();
            }

            var accountRepository = new AccountRepository(context);
            var criteria = new SearchContactsAccountCriteria
            {
                Type = type,
            };
            if (!string.IsNullOrEmpty(sorting))
            {
                criteria.Sorting = new Sorting
                {
                    Field = sorting,
                    Descending = descending,
                };

                if (descending)
                {
                    resultExpected = resultExpected.OrderBy(c => c.GetType().GetProperty(sorting)).ToList();
                }
                else
                {
                    resultExpected = resultExpected.OrderByDescending(c => c.GetType().GetProperty(sorting)).ToList();
                }
            }

            var pagination = new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

            // Act
            var contacts = await accountRepository.GetContactsAccountAsync(accountMock.AccountId, criteria, pagination);

            // Assert
            Assert.Equivalent(resultExpected, contacts.Items);
        }

        [Fact]
        public async Task GetContactsAccountAsync_WhenSortingCriteriaIsInvalid_ShouldThrowBadRequestExceptionEsync()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);

            _fixture.Customizations.Add(new ContactEntityTypeSpecimenBuilder());
            var resultExpected = new List<Contact>();

            var accountMock = _fixture.Build<AccountEntity>()
                                           .Without(a => a.Delegation)
                                           .Without(a => a.RoleEntity)
                                           .Create();

            var accountRepository = new AccountRepository(context);
            var criteria = new SearchContactsAccountCriteria
            {
                Type = It.IsAny<ContactType>(),
            };

            criteria.Sorting = new Sorting
            {
                Field = "bad",
            };

            // Act
            Task Accounts() => accountRepository.GetContactsAccountAsync(accountMock.AccountId, criteria, new Pagination());

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(Accounts);
        }

        [Fact]
        public async Task GetAssociatedContactsAsync_ShouldReturnsContacts()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 999
                };
                var resultExpected = new List<Contact>();
                var accountsMock = _fixture.Create<List<AccountEntity>>();
                var contactAdmin = _fixture.Build<ContactEntity>()
                                           .Without(c => c.DelegationEntityDelegatee)
                                           .Without(c => c.DelegationEntityDelegator)
                                           .Without(c => c.RoleEntity)
                                           .Without(c => c.ContactGlobalUniqueId)
                                           .Create();

                contactAdmin.Type = "customer";
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

                    resultExpected.AddRange(accountsMock[i].RoleEntity
                        .Where(x => x.Contact.Type == "customer")
                        .Select(x => x.Contact.ToContact()!));
                }

                var accountRepository = new AccountRepository(context);

                var request = new GetAssociatedContactsRequest
                {
                    Search = string.Empty,
                    ContactType = ContactType.Customer,
                };

                // Act
                var contactByAdmin = await accountRepository.GetAssociatedContactsAsync(contactAdmin.ContactId, request, pagination);
                var expected = resultExpected.Select(x => x.ContactId).Distinct();

                // Assert
                Assert.Equivalent(expected, contactByAdmin.Items?.Select(x => x.ContactId));
            }
        }

        [Fact]
        public async Task GetAssociatedContactsAsync_ShouldThrow_NotFoundException()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                // Arrange
                var pagination = new Pagination
                {
                    PageNumber = 1,
                    PageSize = 999
                };
                var accountsMock = _fixture.Create<List<AccountEntity>>();

                context.AccountEntity.AddRange(accountsMock);
                context.SaveChanges();

                var accountRepository = new AccountRepository(context);

                var request = new GetAssociatedContactsRequest
                {
                    Search = string.Empty,
                    ContactType = ContactType.Customer,
                };

                // Act
                Task ContactAdmin() => accountRepository.GetAssociatedContactsAsync(123, request, pagination);

                // Assert
                await Assert.ThrowsAsync<NotFoundException>(ContactAdmin);
            }
        }
    }
}
