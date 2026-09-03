// <copyright file="AccountRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Context;
using Pulse.Account.Infrastructure.Tests.Helpers;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class AccountRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public AccountRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Theory]
    [InlineData("199900046522")]
    [InlineData("test sca")]
    [InlineData("firstuser")]
    [InlineData("lastuser")]
    [InlineData("firstlastuser@test.fr")]
    public async Task GetAccountListSearch_Should_ReturnsOkResultAsync(string criteria)
    {
        using (var context = new TestAccountContext(_dbContextOptions))
        {
            // Arrange
            var contactEntity = new ContactEntity
            {
                Type = "1",
                FirstName = "firstUser",
                LastName = "lastUser",
                Email = "firstLastUser@test.fr",
                CreationDate = DateTime.Now,
                PersonaName = "toto",
                IsActive = true,
            };

            var roleEntity = new List<RoleEntity>
            {
                new()
                {
                    IsSignatory = true,
                    Contact = contactEntity,
                }
            };

            var accountEntity = new AccountEntity
            {
                RoleEntity = roleEntity,
                AccountNumber = "199900046522",
                LegalName = "test sca",
                Hub = new HubEntity { HubId = 1, HubName = "HubName" },
                CreatedBy = "me",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString()
            };

            var deploymentENtity = new DeploymentEntity
            {
                Account = accountEntity,
                Status = 1,
            };

            context.DeploymentEntity.AddRange(deploymentENtity);
            await context.SaveChangesAsync();

            var accountRepository = new AccountRepository(context);
            var contactId = accountEntity.RoleEntity.Select(role => role.ContactId).FirstOrDefault();
            var accountObject = accountEntity.MapToAccount(contactId);
            Paging<AccountModel> accountPaging = new Paging<AccountModel>()
            {
                CurrentPage = 1,
                Items = new List<AccountModel> { accountObject! },
                TotalItems = 1,
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
            Assert.Equal(accounts.TotalItems, accountPaging.TotalItems);
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
                                            .With(c => c.Type, "1")
                                            .Create();

            for (int i = 0; i < 3; i++)
            {
                var deploimentEntityMock = _fixture.Build<DeploymentEntity>()
                    .With(a => a.Status, 1)
                    .Without(a => a.Account)
                    .Create();
                var accountMock = _fixture.Build<AccountEntity>()
                                                .With(a => a.IsActive, true)
                                                .Without(a => a.Delegation)
                                                .Without(a => a.RoleEntity)
                                                .Without(a => a.OfferEligibilityEntity)
                                                .Without(a => a.DematModalClosureEntity)
                                                .With(a => a.DeploymentEntity, deploimentEntityMock)
                                                .Create();

                var roleMock = _fixture.Build<RoleEntity>()
                                        .With(e => e.ContactId, contactMock.ContactId)
                                        .With(e => e.Contact, contactMock)
                                        .With(e => e.AccountId, accountMock.AccountId)
                                        .With(e => e.Account, accountMock)
                                        .Create();

                accountMock.RoleEntity.Add(roleMock);

                context.AccountEntity.Add(accountMock);
                await context.SaveChangesAsync();

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
            Assert.Equal(accountPaging.TotalPage, accounts.TotalPage);
            Assert.Equal(accountPaging.TotalItems, accounts.TotalItems);
            Assert.Equal(accountPaging.CurrentPage, accounts.CurrentPage);
        }
    }

    [Theory]
    [InlineData(15)]
    public async Task GetAllAccounts_Should_ReturnsOkResultAsync(int pageSize)
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var resultExpected = new List<AccountModel>();
            var mockedContacts = _fixture.Build<ContactEntity>()
                                            .Without(c => c.DelegationEntityDelegatee)
                                            .Without(c => c.DelegationEntityDelegator)
                                            .Without(c => c.RoleEntity)
                                            .Without(c => c.ContactGlobalUniqueId)
                                            .Without(c => c.RoleLabelEntityContact)
                                            .Without(c => c.RoleLabelEntityCreatedByNavigation)
                                            .With(c => c.Type, "1")
                                            .CreateMany(2)
                                            .ToList();
            for (int i = 0; i < 3; i++)
            {
                var deploimentEntityMock = _fixture.Build<DeploymentEntity>()
                    .With(a => a.Status, 1)
                    .Create();
                var accountMock = _fixture.Build<AccountEntity>()
                                                .Without(a => a.Delegation)
                                                .Without(a => a.RoleEntity)
                                                .Without(a => a.RoleLabelEntity)
                                                .Without(a => a.OfferEligibilityEntity)
                                                .Without(a => a.DematModalClosureEntity)
                                                .With(a => a.IsActive, true)
                                                .With(a => a.DeploymentEntity, deploimentEntityMock)
                                                .Create();

                var firstRoleMock = _fixture.Build<RoleEntity>()
                                        .With(e => e.ContactId, mockedContacts[0].ContactId)
                                        .With(e => e.Contact, mockedContacts[0])
                                        .With(e => e.AccountId, accountMock.AccountId)
                                        .With(e => e.Account, accountMock)
                                        .Create();

                accountMock.RoleEntity.Add(firstRoleMock);
                context.AccountEntity.Add(accountMock);
                await context.SaveChangesAsync();
                resultExpected.Add(accountMock.MapToAccount(mockedContacts[0].ContactId)!);
            }

            for (int i = 0; i < 3; i++)
            {
                var deploimentEntityMock = _fixture.Build<DeploymentEntity>()
                    .With(a => a.Status, 1)
                    .Create();
                var accountMock = _fixture.Build<AccountEntity>()
                                                .Without(a => a.Delegation)
                                                .Without(a => a.RoleEntity)
                                                .Without(a => a.RoleLabelEntity)
                                                .Without(a => a.OfferEligibilityEntity)
                                                .Without(a => a.DematModalClosureEntity)
                                                .With(a => a.IsActive, true)
                                                .With(a => a.DeploymentEntity, deploimentEntityMock)
                                                .Create();
                var secondRoleMock = _fixture.Build<RoleEntity>()
                                        .With(e => e.ContactId, mockedContacts[1].ContactId)
                                        .With(e => e.Contact, mockedContacts[1])
                                        .With(e => e.AccountId, accountMock.AccountId)
                                        .With(e => e.Account, accountMock)
                                        .Create();
                accountMock.RoleEntity.Add(secondRoleMock);
                context.AccountEntity.Add(accountMock);
                await context.SaveChangesAsync();
                resultExpected.Add(accountMock.MapToAccount(mockedContacts[0].ContactId)!);
            }

            var accountRepository = new AccountRepository(context);
            Paging<AccountModel> accountPaging = new Paging<AccountModel>()
            {
                CurrentPage = 1,
                Items = resultExpected!,
                TotalItems = resultExpected.Count,
                TotalPage = Paginator.GetTotalPages(resultExpected.Count, pageSize)
            };
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var accounts = await accountRepository.GetAllAccountsAsync(null, pagination, new SearchAccountCriteria());

            // Assert
            Assert.Equal(accountPaging.TotalPage, accounts.TotalPage);
            Assert.Equal(accountPaging.TotalItems, accounts.TotalItems);
            Assert.Equal(accountPaging.CurrentPage, accounts.CurrentPage);
        }
    }

    [Theory]
    [InlineData(SortingConstants.COMPANYNAME, false, "test sca 1")]
    [InlineData(SortingConstants.CUSTOMERCODE, false, "test sca 3")]
    [InlineData(SortingConstants.LEADER, false, "test sca 3")]
    [InlineData(SortingConstants.CITY, false, "test sca 1")]
    [InlineData(SortingConstants.EMAIL, false, "test sca 3")]
    [InlineData(SortingConstants.LASTACTIVITYDATE, false, "test sca 1")]
    public async Task GetAccountsAsync_SortsCorrectly(string field, bool descending, string expectedFirst)
    {
        using (var context = new TestAccountContext(_dbContextOptions))
        {
            var contactEntity = new ContactEntity
            {
                ContactId = 1,
                Type = "Customer",
                FirstName = "firstUser",
                LastName = "lastUser",
                Email = "firstLastUser@test.fr",
                CreationDate = DateTime.Now,
                PersonaName = "toto",
                IsActive = true,
            };
            var contactEntity2 = new ContactEntity
            {
                ContactId = 2,
                Type = "Customer",
                FirstName = "firstUser",
                LastName = "castUser",
                Email = "dirstLastUser@test.fr",
                CreationDate = DateTime.Now,
                PersonaName = "toto",
                IsActive = true,
            };

            var addressEntity = new AddressEntity
            {
                Country = "France",
                City = "Paris"
            };
            var addressEntity2 = new AddressEntity
            {
                Country = "France",
                City = "Toulouse"
            };

            var accountEntity = new AccountEntity
            {
                AccountId = 1,
                AccountNumber = "199900046524",
                LegalName = "test sca 1",
                Hub = new HubEntity { HubId = 1, HubName = "HubName" },
                CreatedBy = "me",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                AddressEntity = new List<AddressEntity> { addressEntity },
            };
            var accountEntity2 = new AccountEntity
            {
                AccountId = 2,
                AccountNumber = "199900046523",
                LegalName = "test sca 3",
                Hub = new HubEntity { HubId = 2, HubName = "HubName" },
                CreatedBy = "me",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                AddressEntity = new List<AddressEntity> { addressEntity2 },
            };

            var deploymentENtity = new DeploymentEntity
            {
                Account = accountEntity,
                Status = 1,
            };
            var deploymentENtity2 = new DeploymentEntity
            {
                Account = accountEntity2,
                Status = 1,
            };

            var roleEntities = new List<RoleEntity>
            {
                new()
                {
                    IsSignatory = true,
                    Contact = contactEntity,
                    Account = accountEntity,
                    LastActivityDate = DateTime.Now.AddMonths(-1),
                },
                new()
                {
                    IsSignatory = false,
                    Contact = contactEntity,
                    Account = accountEntity2,
                    LastActivityDate = DateTime.Now.AddDays(-1),
                },
                new()
                {
                    IsSignatory = true,
                    Contact = contactEntity2,
                    Account = accountEntity2,
                    LastActivityDate = DateTime.Now,
                }
            };

            context.RoleEntity.AddRange(roleEntities);
            context.DeploymentEntity.AddRange([deploymentENtity, deploymentENtity2]);
            await context.SaveChangesAsync();

            var accountRepository = new AccountRepository(context);
            var criteria = new SearchAccountCriteria
            {
                ContactId = 1,
                Sorting = new Sorting { Field = field, Descending = descending },
            };

            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            var result = await accountRepository.GetAccountsAsync(criteria, pagination);

            Assert.NotNull(result);
            Assert.True(result.Items.Count() > 0);
            Assert.Equal(expectedFirst, result.Items.First().LegalName);
        }
    }

    [Fact]
    public async Task GetAccountsAsync_ThrowException()
    {
        using (var context = new TestAccountContext(_dbContextOptions))
        {
            var contactEntity = new ContactEntity
            {
                ContactId = 1,
                Type = "Customer",
                FirstName = "firstUser",
                LastName = "lastUser",
                Email = "firstLastUser@test.fr",
                CreationDate = DateTime.Now,
                PersonaName = "toto",
                IsActive = true,
            };
            var contactEntity2 = new ContactEntity
            {
                ContactId = 2,
                Type = "Customer",
                FirstName = "firstUser",
                LastName = "castUser",
                Email = "dirstLastUser@test.fr",
                CreationDate = DateTime.Now,
                PersonaName = "toto",
                IsActive = true,
            };

            var addressEntity = new AddressEntity
            {
                Country = "France",
                City = "Paris"
            };
            var addressEntity2 = new AddressEntity
            {
                Country = "France",
                City = "Toulouse"
            };

            var accountEntity = new AccountEntity
            {
                AccountId = 1,
                AccountNumber = "199900046524",
                LegalName = "test sca 1",
                Hub = new HubEntity { HubId = 1, HubName = "HubName" },
                CreatedBy = "me",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                AddressEntity = new List<AddressEntity> { addressEntity },
            };
            var accountEntity2 = new AccountEntity
            {
                AccountId = 2,
                AccountNumber = "199900046523",
                LegalName = "test sca 3",
                Hub = new HubEntity { HubId = 2, HubName = "HubName" },
                CreatedBy = "me",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                AddressEntity = new List<AddressEntity> { addressEntity2 },
            };

            var deploymentENtity = new DeploymentEntity
            {
                Account = accountEntity,
                Status = 1,
            };
            var deploymentENtity2 = new DeploymentEntity
            {
                Account = accountEntity2,
                Status = 1,
            };

            var roleEntities = new List<RoleEntity>
            {
                new()
                {
                    IsSignatory = true,
                    Contact = contactEntity,
                    Account = accountEntity
                },
                new()
                {
                    IsSignatory = false,
                    Contact = contactEntity,
                    Account = accountEntity2
                },
                new()
                {
                    IsSignatory = true,
                    Contact = contactEntity2,
                    Account = accountEntity2
                }
            };

            context.RoleEntity.AddRange(roleEntities);
            context.DeploymentEntity.AddRange([deploymentENtity, deploymentENtity2]);
            await context.SaveChangesAsync();

            var accountRepository = new AccountRepository(context);
            var criteria = new SearchAccountCriteria
            {
                ContactId = 1,
                Sorting = new Sorting { Descending = true, Field = "hahah" }
            };

            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            Task Accounts() => accountRepository.GetAccountsAsync(criteria, pagination);

            await Assert.ThrowsAsync<BadRequestException>(Accounts);
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
                                            .With(c => c.Type, "1")
                                            .Create();

            var deploymentMockActive = _fixture.Build<DeploymentEntity>()
                .With(a => a.Status, 1)
                .Create();

            var accountMock = _fixture.Build<AccountEntity>()
                                            .Without(a => a.Delegation)
                                            .Without(a => a.RoleEntity)
                                            .Without(a => a.OfferEligibilityEntity)
                                            .Without(a => a.DematModalClosureEntity)
                                            .With(a => a.DeploymentEntity, deploymentMockActive)
                                            .With(a => a.IsActive, true)
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
                                                .Without(a => a.OfferEligibilityEntity)
                                                .Without(a => a.DematModalClosureEntity)
                                                .With(a => a.DeploymentEntity, deploymentMock)
                                                .Create();
            context.AccountEntity.Add(accountMockInactive);
            await context.SaveChangesAsync();

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
    public async Task GetAccountSummary_Should_ReturnsOkResultAsync()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountsModel = _fixture.Create<List<AccountEntity>>();
            var contactId = 1;
            var accountFirst = accountsModel[0];
            var accountSummary = accountFirst?.MapToAccountSummary(contactId);
            context.AccountEntity.AddRange(accountsModel);
            await context.SaveChangesAsync();
            var accountRepository = new AccountRepository(context);

            // Act
            var accounts = await accountRepository.GetAccountSummaryAsync(contactId, accountFirst!.AccountId);

            // Assert
            Assert.Equal(accountSummary?.AccountNumber, accounts!.AccountNumber);
            Assert.Equal(accountSummary?.AccountId, accounts.AccountId);
            Assert.Equal(accountSummary?.LegalName, accounts.LegalName);
            Assert.Equal(accountSummary?.IsClarityVisible, accounts.IsClarityVisible);
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
    public async Task GetAccountAsync_Should_ReturnsAccountAsync_WhenSiretNull()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountsModel = _fixture.Create<List<AccountEntity>>();
            accountsModel[0].Siret = null;
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
    public async Task UpdateAccount_ShouldThrowNotFoundException_IfAccountIsNotFound()
    {
        int accountId = -1;

        AccountDetail accountDetail = _fixture.Build<AccountDetail>().Create();
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountRepository = new AccountRepository(context);

            var action = async () => await accountRepository.UpdateAccountAsync(accountId, accountDetail);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }

    [Fact]
    public async Task GetContactsAccountAsync_ShouldNotThrowNotFoundEXception_IfQueryResultIsNull()
    {
        int accountId = -1;
        SearchContactsAccountCriteria criteria = _fixture.Build<SearchContactsAccountCriteria>()
            .With(x => x.Sorting, new Sorting { Field = SortingConstants.NAME, Descending = true })
            .Create();
        Pagination pagination = _fixture.Build<Pagination>()
            .With(x => x.PageNumber, 1)
            .With(x => x.PageSize, 15)
            .Create();
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountRepository = new AccountRepository(context);

            var result = await accountRepository.GetContactsAccountAsync(accountId, criteria, pagination);

            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task UpdateAccount_Should_ReturnsOkResultAsync()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountsModel = _fixture.Build<AccountEntity>().Without(a => a.OfferEligibilityEntity).Without(a => a.DematModalClosureEntity).With(a => a.IsActive, true).Create();
            accountsModel.OfficeId = accountsModel.Office!.OfficeId;
            accountsModel.Office = null;
            var accountDetail = accountsModel?.MapToAccountDetail();

            if (accountDetail?.Accounting != null)
            {
                accountDetail.Accounting.TaxationSystem = "Impot sur le revenu";
            }

            context.AccountEntity.AddRange(accountsModel!);
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

        var resultExpected = new List<Contact>();
        var contactsMock = new List<ContactEntity>
        {
            new()
            {
                ContactId = 11,
                Email = "email1",
                FirstName = "firstName1",
                LastName = "lastName1",
                PersonaName = "persona1",
                Type = "collaborator1",
                IsActive = true,
                RoleLabelEntityContact = new List<RoleLabelEntity>
                {
                    new()
                    {
                        AccountId = 10,
                        ContactId = 11,
                        LabelId = 1
                    },
                    new()
                    {
                        AccountId = 10,
                        ContactId = 11,
                        LabelId = 2
                    }
                },
                RoleEntity = new List<RoleEntity>
                {
                    new()
                    {
                        AccountId = 10,
                        ContactId = 11,
                        ActionLevel = 0,
                        IsCustomerRelation = true,
                        IsDelegation = true,
                        IsFavorite = true,
                        IsSignatory = true,
                    }
                }
            },
            new()
            {
                ContactId = 12,
                Email = "email2",
                FirstName = "firstName2",
                LastName = "lastName2",
                PersonaName = "persona2",
                Type = "collaborator2",
                IsActive = true,
                RoleLabelEntityContact = new List<RoleLabelEntity>
                {
                    new()
                    {
                        AccountId = 10,
                        ContactId = 12,
                        Label = new()
                        {
                            LabelId = 1,
                            Code = "code1",
                            CollaboratorLabel = "collabLabel1",
                            CustomerLabel = "clientLabel1",
                            Business = "ESC",
                            IsVisible = true
                        }
                    },
                    new()
                    {
                        AccountId = 10,
                        ContactId = 12,
                        Label = new()
                        {
                            LabelId = 2,
                            Code = "code2",
                            CollaboratorLabel = "collabLabel2",
                            CustomerLabel = "clientLabel2",
                            Business = "ESC",
                            IsVisible = true
                        }
                    }
                },
                RoleEntity = new List<RoleEntity>
                {
                    new()
                    {
                        AccountId = 10,
                        ContactId = 12,
                        ActionLevel = 1,
                        IsCustomerRelation = false,
                        IsDelegation = false,
                        IsFavorite = false,
                        IsSignatory = false,
                    }
                }
            }
        };

        if (type == null)
        {
            resultExpected.AddRange(contactsMock.Select(c => c.MapToContact(10))!);
        }
        else
        {
            resultExpected.AddRange(contactsMock.Where(c => c.Type == type.ToString()!.ToLower()).Select(c => c.MapToContact(10))!);
        }

        var accountMock = new AccountEntity
        {
            AccountId = 10,
            AccountNumber = "ACC-10",
            LegalName = "Test Account",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true
        };
        context.AccountEntity.Add(accountMock);
        context.ContactEntity.AddRange(contactsMock);
        await context.SaveChangesAsync();

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
        var contacts = await accountRepository.GetContactsAccountAsync(10, criteria, pagination);

        // Assert
        Assert.Equivalent(resultExpected, contacts.Items);
    }

    [Fact]
    public async Task GetContactsAccountAsync_WhenSortingCriteriaIsInvalid_ShouldThrowBadRequestExceptionEsync()
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        _fixture.Customizations.Add(new ContactEntityTypeSpecimenBuilder());

        var accountMock = _fixture.Build<AccountEntity>()
                                        .Without(a => a.Delegation)
                                        .Without(a => a.RoleEntity)
                                        .Without(a => a.OfferEligibilityEntity)
                                        .Without(a => a.DematModalClosureEntity)
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
        var result = await Assert.ThrowsAsync<BadRequestException>(Accounts);
        Assert.Equal("ACC019", result.Code);
        Assert.Equal("Le champ demandé bad n'existe pas", result.Message);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    public async Task GetContactsAccountAsync_WhenIsCustomerRelationCriteria_ShouldFilterOnIsCustomerRelationContacts(bool isCustomerRelation, int expectedId)
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        var roleIsCustomerRelation = new RoleEntity
        {
            AccountId = 1,
            ContactId = 1,
            IsCustomerRelation = true
        };
        var roleIsNotCustomerRelation = new RoleEntity
        {
            AccountId = 1,
            ContactId = 2,
            IsCustomerRelation = false
        };

        var contactIsCustomerRelation = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();
        var contactIsNotCustomerRelation = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 2)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();

        var accountMock = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-1",
            LegalName = "Test Account",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true
        };
        context.AccountEntity.Add(accountMock);
        context.ContactEntity.AddRange(new List<ContactEntity> { contactIsCustomerRelation, contactIsNotCustomerRelation });
        context.RoleEntity.AddRange(new List<RoleEntity> { roleIsCustomerRelation, roleIsNotCustomerRelation });
        await context.SaveChangesAsync();

        var criteria = new SearchContactsAccountCriteria { IsCustomerRelation = isCustomerRelation };

        var accountRepository = new AccountRepository(context);

        // Act
        var result = await accountRepository.GetContactsAccountAsync(1, criteria, new Pagination { PageNumber = 1, PageSize = 100 });

        // Assert
        Assert.NotNull(result);

        var contacts = result.Items;
        Assert.NotNull(contacts);
        Assert.Single(contacts);
        Assert.Equal(expectedId, contacts.First().ContactId);
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
            var deploymentMock = _fixture.Build<DeploymentEntity>()
                                        .Without(d => d.Account)
                                        .Create();
            var accountsMock = _fixture.Build<AccountEntity>()
                                        .Without(a => a.RoleEntity)
                                        .Without(a => a.Delegation)
                                        .Without(a => a.OfferEligibilityEntity)
                                        .Without(a => a.DematModalClosureEntity)
                                        .With(a => a.DeploymentEntity, deploymentMock)
                                        .CreateMany(1)
                                        .ToList();
            var contactAdmin = _fixture.Build<ContactEntity>()
                                        .Without(c => c.DelegationEntityDelegatee)
                                        .Without(c => c.DelegationEntityDelegator)
                                        .Without(c => c.RoleEntity)
                                        .With(c => c.IsActive, true)
                                        .With(c => c.Type, ContactType.Customer.ToString())
                                        .Create();

            context.AccountEntity.AddRange(accountsMock);

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

                resultExpected.AddRange(accountsMock[i].RoleEntity
                    .Where(x => x.Contact.Type == ContactType.Customer.ToString())
                    .Select(x => x.Contact.ToContact()!));
            }

            await context.SaveChangesAsync();

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
    public async Task GetAssociatedContactsAsync_ShouldThrowNotFoundException_IfQueryGetAccountByContactIsEmpty()
    {
        int contactId = -1;
        GetAssociatedContactsRequest request = new GetAssociatedContactsRequest() { ContactType = ContactType.Customer, Search = string.Empty };
        Pagination pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        using (var context = new AccountContext(_dbContextOptions))
        {
            var accountRepository = new AccountRepository(context);
            var action = async () => await accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);

            await action.Should().ThrowAsync<NotFoundException>();
        }
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_ShouldSearchQueryFirstLastPersonaName_IfRequestSearchIsNotEmpty()
    {
        AccountEntity accountEntity = _fixture.Build<AccountEntity>()
            .With(x => x.AccountId, 1)
            .Without(x => x.RoleEntity)
            .Without(x => x.PhoneEntity)
            .Without(x => x.Delegation)
            .Without(a => a.OfferEligibilityEntity)
            .Without(a => a.DematModalClosureEntity)
            .Create();
        List<ContactEntity> contacts = _fixture.Build<ContactEntity>()
            .Without(x => x.RoleEntity)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .With(x => x.Type, ContactType.Collaborator.ToString())
            .With(c => c.IsActive, true)
            .CreateMany(10).ToList();

        List<RoleEntity> roles = new List<RoleEntity>();
        foreach (var contact in contacts)
        {
            RoleEntity role = new RoleEntity { AccountId = 1, ContactId = contact.ContactId };
            roles.Add(role);
        }

        int contactId = contacts.FirstOrDefault().ContactId;
        string contactName = contacts.FirstOrDefault().FirstName;

        var request = new GetAssociatedContactsRequest { ContactType = ContactType.Collaborator, Search = contactName };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        using (var context = new AccountContext(_dbContextOptions))
        {
            context.AccountEntity.Add(accountEntity);
            context.ContactEntity.AddRange(contacts);
            context.RoleEntity.AddRange(roles);
            await context.SaveChangesAsync();

            var repos = new AccountRepository(context);
            var result = await repos.GetAssociatedContactsAsync(contacts.FirstOrDefault().ContactId, request, pagination);

            request.Search.Should().NotBeEmpty();
            request.Search.Should().BeEquivalentTo(contactName);
            result.Items.Count().Should().BeGreaterThan(0);
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
            await context.SaveChangesAsync();

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

    [Fact]
    public async Task GetAccountsAsync_WithDeploymentStatus_ShouldFilterResults()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var deploymentStatus = DeploymentStatus.Connected;
            var contactId = 123;
            var accountRepository = new AccountRepository(context);

            // Add test data with different deployment statuses
            // ... (add test data setup here)
            var searchCriteria = new SearchAccountCriteria
            {
                DeploymentStatus = new List<int> { (int)deploymentStatus },
                ContactId = contactId
            };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await accountRepository.GetAccountsAsync(searchCriteria, pagination);

            // Assert
            Assert.All(result.Items, item => Assert.Equal((int)deploymentStatus, item.Deployment.Status));
        }
    }

    [Fact]
    public async Task GetAccountsAsync_WithMultipleDeploymentStatuses_ShouldReturnUnion()
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();

        var connectedAccount = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-1",
            LegalName = "Connected",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = (int)DeploymentStatus.Connected }
        };
        var toDeployAccount = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-2",
            LegalName = "ToDeploy",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = (int)DeploymentStatus.ToDeploy }
        };
        var revokedAccount = new AccountEntity
        {
            AccountId = 3,
            AccountNumber = "ACC-3",
            LegalName = "Revoked",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = (int)DeploymentStatus.Revoked }
        };

        context.ContactEntity.Add(contact);
        context.RoleEntity.AddRange(
            new RoleEntity { Account = connectedAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = toDeployAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = revokedAccount, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accountRepository = new AccountRepository(context);

        // Act : filtrer sur Connected ET ToDeploy => union, en excluant Revoked
        var result = await accountRepository.GetAccountsAsync(
            new SearchAccountCriteria
            {
                ContactId = contact.ContactId,
                DeploymentStatus = new List<int> { (int)DeploymentStatus.Connected, (int)DeploymentStatus.ToDeploy }
            },
            new Pagination { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.All(result.Items, item => Assert.Contains(item.Deployment.Status, new[] { (int)DeploymentStatus.Connected, (int)DeploymentStatus.ToDeploy }));
        Assert.DoesNotContain(result.Items, item => item.Deployment.Status == (int)DeploymentStatus.Revoked);
    }

    [Theory]
    [InlineData("Tenue")]
    [InlineData("Revision")]
    public async Task GetAccountsAsync_WithMissionType_ShouldFilterResults(string missionType)
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Create();

        var tenueAccount = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-TENUE-001",
            LegalName = "Tenue Account",
            AccountType = "CLIENT",
            MissionType = "Tenue",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var revisionAccount = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-REVISION-001",
            LegalName = "Revision Account",
            AccountType = "CLIENT",
            MissionType = "Revision",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.ContactEntity.Add(contact);
        context.RoleEntity.AddRange(
            new RoleEntity { Account = tenueAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = revisionAccount, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accountRepository = new AccountRepository(context);

        // Act
        var result = await accountRepository.GetAccountsAsync(
            new SearchAccountCriteria { ContactId = contact.ContactId, MissionType = new List<string> { missionType } },
            new Pagination { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.Single(result.Items);
        Assert.All(result.Items, item => Assert.Equal(missionType, item.MissionType));
    }

    [Fact]
    public async Task GetAccountsAsync_WithMultipleMissionTypes_ShouldReturnUnion()
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Create();

        var tenueAccount = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-1",
            LegalName = "Tenue Account",
            AccountType = "CLIENT",
            MissionType = "Tenue",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var revisionAccount = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-2",
            LegalName = "Revision Account",
            AccountType = "CLIENT",
            MissionType = "Revision",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var otherAccount = new AccountEntity
        {
            AccountId = 3,
            AccountNumber = "ACC-3",
            LegalName = "Other Account",
            AccountType = "CLIENT",
            MissionType = "Audit",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.ContactEntity.Add(contact);
        context.RoleEntity.AddRange(
            new RoleEntity { Account = tenueAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = revisionAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = otherAccount, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accountRepository = new AccountRepository(context);

        // Act : filtrer sur Tenue ET Revision => union des deux, en excluant les autres
        var result = await accountRepository.GetAccountsAsync(
            new SearchAccountCriteria { ContactId = contact.ContactId, MissionType = new List<string> { "Tenue", "Revision" } },
            new Pagination { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.All(result.Items, item => Assert.Contains(item.MissionType, new[] { "Tenue", "Revision" }));
        Assert.DoesNotContain(result.Items, item => item.MissionType == "Audit");
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissionTypeNone_ShouldReturnAccountsWithoutMissionType()
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Create();

        var tenueAccount = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-1",
            LegalName = "Tenue Account",
            AccountType = "CLIENT",
            MissionType = "Tenue",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var nullMissionAccount = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-2",
            LegalName = "Null Mission Account",
            AccountType = "CLIENT",
            MissionType = null,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var emptyMissionAccount = new AccountEntity
        {
            AccountId = 3,
            AccountNumber = "ACC-3",
            LegalName = "Empty Mission Account",
            AccountType = "CLIENT",
            MissionType = string.Empty,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.ContactEntity.Add(contact);
        context.RoleEntity.AddRange(
            new RoleEntity { Account = tenueAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = nullMissionAccount, ContactId = contact.ContactId },
            new RoleEntity { Account = emptyMissionAccount, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accountRepository = new AccountRepository(context);

        // Act : MissionType = None => Accounts sans mission renseignée (null ou vide)
        var result = await accountRepository.GetAccountsAsync(
            new SearchAccountCriteria { ContactId = contact.ContactId, MissionType = new List<string> { MissionType.None.ToString() } },
            new Pagination { PageNumber = 1, PageSize = 10 });

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.All(result.Items, item => Assert.True(string.IsNullOrEmpty(item.MissionType)));
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissionTypeAndDeploymentStatus_ShouldApplyBothCumulatively()
    {
        // Arrange
        using var context = new AccountContext(_dbContextOptions);

        var contact = _fixture.Build<ContactEntity>()
            .With(c => c.ContactId, 1)
            .With(c => c.IsActive, true)
            .Without(c => c.RoleEntity)
            .Without(c => c.RoleLabelEntityContact)
            .Without(c => c.RoleLabelEntityCreatedByNavigation)
            .Without(c => c.DelegationEntityDelegatee)
            .Without(c => c.DelegationEntityDelegator)
            .Create();

        var matching = new AccountEntity
        {
            AccountId = 1,
            AccountNumber = "ACC-1",
            LegalName = "Matching",
            AccountType = "CLIENT",
            MissionType = "Tenue",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = (int)DeploymentStatus.Connected }
        };
        var wrongStatus = new AccountEntity
        {
            AccountId = 2,
            AccountNumber = "ACC-2",
            LegalName = "WrongStatus",
            AccountType = "CLIENT",
            MissionType = "Tenue",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = (int)DeploymentStatus.ToDeploy }
        };
        var wrongMission = new AccountEntity
        {
            AccountId = 3,
            AccountNumber = "ACC-3",
            LegalName = "WrongMission",
            AccountType = "CLIENT",
            MissionType = "Revision",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = (int)DeploymentStatus.Connected }
        };

        context.ContactEntity.Add(contact);
        context.RoleEntity.AddRange(
            new RoleEntity { Account = matching, ContactId = contact.ContactId },
            new RoleEntity { Account = wrongStatus, ContactId = contact.ContactId },
            new RoleEntity { Account = wrongMission, ContactId = contact.ContactId });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accountRepository = new AccountRepository(context);

        // Act : statut ET type de mission doivent se cumuler (ET)
        var result = await accountRepository.GetAccountsAsync(
            new SearchAccountCriteria
            {
                ContactId = contact.ContactId,
                MissionType = new List<string> { "Tenue" },
                DeploymentStatus = new List<int> { (int)DeploymentStatus.Connected }
            },
            new Pagination { PageNumber = 1, PageSize = 10 });

        // Assert : seul le compte respectant les deux critères est retourné
        Assert.Single(result.Items);
        Assert.Equal(matching.AccountId, result.Items.Single().AccountId);
    }

    [Fact]
    public async Task GetAllAccountsAsync_WithAccountNumber_ShouldFilterResults()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountNumber = "123456";
            var accountRepository = new AccountRepository(context);

            // Add test data with different account numbers
            // ... (add test data setup here)
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await accountRepository.GetAllAccountsAsync(accountNumber, pagination, new SearchAccountCriteria());

            // Assert
            Assert.All(result.Items, item => Assert.Contains(accountNumber, item.AccountNumber));
        }
    }

    [Theory]
    [InlineData(SortingConstants.COMPANYNAME, false, "legalName1")]
    [InlineData(SortingConstants.COMPANYNAME, true, "legalName2")]
    [InlineData(SortingConstants.CUSTOMERCODE, false, "legalName1")]
    [InlineData(SortingConstants.CUSTOMERCODE, true, "legalName2")]
    [InlineData(SortingConstants.LEADER, false, "legalName1")]
    [InlineData(SortingConstants.LEADER, true, "legalName2")]
    [InlineData(SortingConstants.EMAIL, false, "legalName1")]
    [InlineData(SortingConstants.EMAIL, true, "legalName2")]
    [InlineData(SortingConstants.CITY, true, "legalName2")]
    [InlineData(SortingConstants.CITY, false, "legalName1")]
    [InlineData(SortingConstants.STATUS, true, "legalName1")]
    [InlineData(SortingConstants.STATUS, false, "legalName1")]
    [InlineData(SortingConstants.LASTACTIVITYDATE, false, "legalName2")]
    [InlineData(SortingConstants.LASTACTIVITYDATE, true, "legalName1")]
    public async Task GetAllAccountsAsync_WithAccountNumber_ShouldSortResults(string field, bool isDescending, string accountLegalnameExpected)
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var contact1 = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 1)
                .With(c => c.FirstName, "fname1")
                .With(c => c.Email, "email1")
                .With(c => c.IsActive, true)
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.RoleLabelEntityCreatedByNavigation)
                .Create();
            var contact2 = _fixture.Build<ContactEntity>()
                .With(c => c.ContactId, 22)
                .With(c => c.FirstName, "fname2")
                .With(c => c.Email, "email2")
                .With(c => c.IsActive, true)
                .Without(c => c.RoleEntity)
                .Without(c => c.DelegationEntityDelegatee)
                .Without(c => c.DelegationEntityDelegator)
                .Without(c => c.RoleLabelEntityContact)
                .Without(c => c.RoleLabelEntityCreatedByNavigation)
                .Create();
            var role1 = _fixture.Build<RoleEntity>()
                .With(r => r.IsSignatory, true)
                .With(r => r.AccountId, 1)
                .With(r => r.ContactId, 1)
                .With(r => r.LastActivityDate, (DateTime?)new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc))
                .Without(r => r.Account)
                .Without(r => r.Contact)
                .Create();
            var address1 = _fixture.Build<AddressEntity>()
                .With(adr => adr.City, "city1")
                .With(adr => adr.AccountId, 1)
                .Without(adr => adr.Account)
                .Without(adr => adr.OfficeEntity)
                .Create();
            var deploi1 = _fixture.Build<DeploymentEntity>()
                .With(d => d.Status, (int)DeploymentStatus.Connected)
                .With(d => d.AccountId, 1)
                .Without(d => d.Account)
                .Create();
            var role2 = _fixture.Build<RoleEntity>()
                .With(r => r.IsSignatory, true)
                .With(r => r.AccountId, 2)
                .With(r => r.ContactId, 22)
                .With(r => r.LastActivityDate, (DateTime?)new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .Without(r => r.Account)
                .Without(r => r.Contact)
                .Create();
            var address2 = _fixture.Build<AddressEntity>()
                .With(adr => adr.City, "city2")
                .With(adr => adr.AccountId, 2)
                .Without(adr => adr.Account)
                .Without(adr => adr.OfficeEntity)
                .Create();
            var deploi2 = _fixture.Build<DeploymentEntity>()
                .With(d => d.Status, (int)DeploymentStatus.Connected)
                .With(d => d.AccountId, 2)
                .Without(d => d.Account)
                .Create();
            var account1 = _fixture.Build<AccountEntity>()
                .With(a => a.AccountId, 1)
                .With(a => a.AccountNumber, "accountNumber1")
                .With(a => a.LegalName, "legalName1")
                .Without(a => a.RoleEntity)
                .Without(a => a.AddressEntity)
                .Without(a => a.DeploymentEntity)
                .Without(a => a.PhoneEntity)
                .Without(a => a.RoleLabelEntity)
                .Without(a => a.Delegation)
                .Without(a => a.OfferEligibilityEntity)
                .Without(a => a.DematModalClosureEntity)
                .With(x => x.IsActive, true)
                .Create();
            var account2 = _fixture.Build<AccountEntity>()
                .With(a => a.AccountId, 2)
                .With(a => a.AccountNumber, "accountNumber2")
                .With(a => a.LegalName, "legalName2")
                .Without(a => a.RoleEntity)
                .Without(a => a.AddressEntity)
                .Without(a => a.DeploymentEntity)
                .Without(a => a.PhoneEntity)
                .Without(a => a.RoleLabelEntity)
                .Without(a => a.Delegation)
                .Without(a => a.OfferEligibilityEntity)
                .Without(a => a.DematModalClosureEntity)
                .With(x => x.IsActive, true)
                .Create();

            context.DeploymentEntity.AddRange(deploi1, deploi2);
            context.AddressEntity.AddRange(address1, address2);
            context.ContactEntity.AddRange(contact1, contact2);
            context.AccountEntity.AddRange(account1, account2);
            context.RoleEntity.AddRange(role1, role2);

            await context.SaveChangesAsync();
            var accountRepository = new AccountRepository(context);

            // Add test data with different account numbers
            // ... (add test data setup here)
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var searchAccountCriteria = new SearchAccountCriteria
            {
                Sorting = new Sorting
                {
                    Descending = isDescending,
                    Field = field
                }
            };

            // Act
            var result = await accountRepository.GetAllAccountsAsync(null, pagination, searchAccountCriteria);

            // Assert
            Assert.Equal(accountLegalnameExpected, result.Items.First().LegalName);
        }
    }

    [Fact]
    public async Task GetAllAccountsAsync_WhenSortingFieldIsUnknown_ShouldThrowBadRequestException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var accountRepository = new AccountRepository(context);
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var searchAccountCriteria = new SearchAccountCriteria
            {
                Sorting = new Sorting
                {
                    Descending = false,
                    Field = "unknownField"
                }
            };

            // Act
            Task Accounts() => accountRepository.GetAllAccountsAsync(null, pagination, searchAccountCriteria);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(Accounts);
        }
    }

    [Fact]
    public async Task GetContactsAccountAsync_WithSearchCriteria_ShouldFilterResults()
    {
        var accountId = 1;
        var searchTerm = "John";

        var account = _fixture.Build<AccountEntity>()
                .With(x => x.AccountId, accountId)
                .Without(x => x.RoleEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.AddressEntity)
                .Without(x => x.DeploymentEntity)
                .Without(a => a.OfferEligibilityEntity)
                .Without(a => a.DematModalClosureEntity)
                .Create();

        var contact = _fixture.Build<ContactEntity>()
            .With(x => x.FirstName, searchTerm)
            .With(x => x.Type, ContactType.Collaborator.ToString())
            .Without(x => x.RoleEntity)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .Create();

        var role = _fixture.Build<RoleEntity>()
            .Without(x => x.Account)
            .Without(x => x.Contact)
            .With(x => x.AccountId, account.AccountId)
            .With(x => x.ContactId, contact.ContactId)
            .Create();

        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            context.AccountEntity.Add(account);
            context.ContactEntity.Add(contact);
            await context.SaveChangesAsync();
            context.RoleEntity.Add(role);
            await context.SaveChangesAsync();

            var accountRepository = new AccountRepository(context);

            // Add test data with different contact names
            var criteria = new SearchContactsAccountCriteria { Search = searchTerm };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await accountRepository.GetContactsAccountAsync(accountId, criteria, pagination);

            // Assert
            Assert.All(result.Items, item =>
                Assert.Contains(searchTerm, $"{item.FirstName} {item.LastName} {item.Email} {item.PersonaName} {item.Office}", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task GetContactsAccountAsync_WithSearchCriteriaForTypeCollaborator_ShouldFilterResultsAndReturnContactsOrderedDescendingByActionLevel()
    {
        var accountId = 1;

        var account = _fixture.Build<AccountEntity>()
                .With(x => x.AccountId, accountId)
                .Without(x => x.RoleEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.AddressEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Office)
                .Without(x => x.Delegation)
                .Without(x => x.RoleLabelEntity)
                .Without(x => x.OfferEligibilityEntity)
                .Without(a => a.OfferEligibilityEntity)
                .Without(a => a.DematModalClosureEntity)
                .With(x => x.IsActive, true)
                .Create();

        var contact1 = _fixture.Build<ContactEntity>()
            .With(x => x.FirstName, "Mostapha")
            .With(x => x.Type, ContactType.Collaborator.ToString())
            .With(x => x.ContactId, 1)
            .Without(x => x.RoleEntity)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .Without(x => x.RoleLabelEntityContact)
            .Without(x => x.RoleLabelEntityCreatedByNavigation)
            .With(x => x.IsActive, true)
            .Create();

        var contact2 = _fixture.Build<ContactEntity>()
            .With(x => x.FirstName, "Nadim")
            .With(x => x.Type, ContactType.Collaborator.ToString())
            .With(x => x.ContactId, 2)
            .Without(x => x.RoleEntity)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .Without(x => x.RoleLabelEntityContact)
            .Without(x => x.RoleLabelEntityCreatedByNavigation)
            .With(x => x.IsActive, true)
            .Create();

        var contact3 = _fixture.Build<ContactEntity>()
            .With(x => x.FirstName, "Awad")
            .With(x => x.Type, ContactType.Collaborator.ToString())
            .With(x => x.ContactId, 3)
            .Without(x => x.RoleEntity)
            .Without(x => x.DelegationEntityDelegatee)
            .Without(x => x.DelegationEntityDelegator)
            .Without(x => x.RoleLabelEntityContact)
            .Without(x => x.RoleLabelEntityCreatedByNavigation)
            .With(x => x.IsActive, true)
            .Create();

        var role1 = _fixture.Build<RoleEntity>()
            .Without(x => x.Account)
            .Without(x => x.Contact)
            .With(x => x.AccountId, account.AccountId)
            .With(x => x.ContactId, contact1.ContactId)
            .With(x => x.IsCustomerRelation, true)
            .With(x => x.ActionLevel, 1)
            .Create();

        var role2 = _fixture.Build<RoleEntity>()
           .Without(x => x.Account)
           .Without(x => x.Contact)
           .With(x => x.AccountId, account.AccountId)
           .With(x => x.ContactId, contact2.ContactId)
           .With(x => x.IsCustomerRelation, true)
           .With(x => x.ActionLevel, 2)
           .Create();

        var role3 = _fixture.Build<RoleEntity>()
           .Without(x => x.Account)
           .Without(x => x.Contact)
           .With(x => x.AccountId, account.AccountId)
           .With(x => x.ContactId, contact3.ContactId)
           .With(x => x.IsCustomerRelation, true)
           .With(x => x.ActionLevel, 3)
           .Create();

        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(new List<ContactEntity>() { contact1, contact2, contact3 });
            await context.SaveChangesAsync();
            context.RoleEntity.AddRange(new List<RoleEntity>() { role1, role2, role3 });
            await context.SaveChangesAsync();

            var accountRepository = new AccountRepository(context);

            // Add test data with different contact names
            var criteria = new SearchContactsAccountCriteria { Type = ContactType.Collaborator };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await accountRepository.GetContactsAccountAsync(accountId, criteria, pagination);

            var firstContact = result.Items.FirstOrDefault();
            var thirdContact = result.Items.LastOrDefault();
            Assert.NotNull(firstContact);
            Assert.Equal(firstContact?.ActionLevel, 3);
            Assert.Equal(thirdContact?.ActionLevel, 1);
        }
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_WithSearch_ShouldFilterResults()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var contactId = 1;
            var searchTerm = "Jane";
            var accountRepository = new AccountRepository(context);

            // Add test data with different associated contacts
            // ... (add test data setup here)
            var request = new GetAssociatedContactsRequest
            {
                Search = searchTerm,
                ContactType = ContactType.Customer
            };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            // Act
            var action = async () => await accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_WithSearch_ShouldFilterResultsOrderByName()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountId = 1;
            var contactId = 1;

            var account = _fixture.Build<AccountEntity>()
                    .With(x => x.AccountId, accountId)
                    .Without(x => x.RoleEntity)
                    .Without(x => x.PhoneEntity)
                    .Without(x => x.AddressEntity)
                    .Without(x => x.DeploymentEntity)
                    .Without(x => x.Office)
                    .Without(x => x.Delegation)
                    .Without(x => x.RoleLabelEntity)
                    .Without(x => x.OfferEligibilityEntity)
                    .Without(x => x.DematModalClosureEntity)
                    .Without(a => a.OfferEligibilityEntity)
                    .With(x => x.IsActive, true)
                    .Create();

            var contact1 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Mostapha")
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, contactId)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var contact2 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Nadim")
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, 2)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var contact3 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Awad")
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, 3)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var role1 = _fixture.Build<RoleEntity>()
                .Without(x => x.Account)
                .Without(x => x.Contact)
                .With(x => x.AccountId, account.AccountId)
                .With(x => x.ContactId, contact1.ContactId)
                .With(x => x.IsCustomerRelation, true)
                .With(x => x.ActionLevel, 1)
                .Create();

            var role2 = _fixture.Build<RoleEntity>()
               .Without(x => x.Account)
               .Without(x => x.Contact)
               .With(x => x.AccountId, account.AccountId)
               .With(x => x.ContactId, contact2.ContactId)
               .With(x => x.IsCustomerRelation, true)
               .With(x => x.ActionLevel, 2)
               .Create();

            var role3 = _fixture.Build<RoleEntity>()
               .Without(x => x.Account)
               .Without(x => x.Contact)
               .With(x => x.AccountId, account.AccountId)
               .With(x => x.ContactId, contact3.ContactId)
               .With(x => x.IsCustomerRelation, true)
               .With(x => x.ActionLevel, 3)
               .Create();

            var searchTerm = string.Empty;

            var request = new GetAssociatedContactsRequest
            {
                Search = searchTerm,
                Sorting = new Sorting
                {
                    Field = SortingConstants.NAME,
                    Descending = false
                },
                ContactType = ContactType.Collaborator
            };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(new List<ContactEntity>() { contact1, contact2, contact3 });
            await context.SaveChangesAsync();
            context.RoleEntity.AddRange(new List<RoleEntity>() { role1, role2, role3 });
            await context.SaveChangesAsync();
            var accountRepository = new AccountRepository(context);

            // Act
            var result = await accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);

            // Assert
            Assert.Equal(3, result.Items!.Count());
            Assert.Equal("Awad", result.Items.ToList()[0].FirstName);
            Assert.Equal("Mostapha", result.Items.ToList()[1].FirstName);
            Assert.Equal("Nadim", result.Items.ToList()[2].FirstName);
        }
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_WithSearch_ShouldFilterResultsOrderByEmailDescending()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountId = 1;
            var contactId = 1;

            var account = _fixture.Build<AccountEntity>()
                    .With(x => x.AccountId, accountId)
                    .Without(x => x.RoleEntity)
                    .Without(x => x.PhoneEntity)
                    .Without(x => x.AddressEntity)
                    .Without(x => x.DeploymentEntity)
                    .Without(x => x.Office)
                    .Without(x => x.Delegation)
                    .Without(x => x.RoleLabelEntity)
                    .Without(x => x.OfferEligibilityEntity)
                    .Without(x => x.DematModalClosureEntity)
                    .Without(a => a.OfferEligibilityEntity)
                    .With(x => x.IsActive, true)
                    .Create();

            var contact1 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Mostapha")
                .With(x => x.Email, "mostapha@abc.com")
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, contactId)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var contact2 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Nadim")
                .With(x => x.Email, "nadim@abc.com")
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, 2)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var contact3 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Awad")
                .With(x => x.Email, "awad@abc.com")
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, 3)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var role1 = _fixture.Build<RoleEntity>()
                .Without(x => x.Account)
                .Without(x => x.Contact)
                .With(x => x.AccountId, account.AccountId)
                .With(x => x.ContactId, contact1.ContactId)
                .With(x => x.IsCustomerRelation, true)
                .With(x => x.ActionLevel, 1)
                .Create();

            var role2 = _fixture.Build<RoleEntity>()
               .Without(x => x.Account)
               .Without(x => x.Contact)
               .With(x => x.AccountId, account.AccountId)
               .With(x => x.ContactId, contact2.ContactId)
               .With(x => x.IsCustomerRelation, true)
               .With(x => x.ActionLevel, 2)
               .Create();

            var role3 = _fixture.Build<RoleEntity>()
               .Without(x => x.Account)
               .Without(x => x.Contact)
               .With(x => x.AccountId, account.AccountId)
               .With(x => x.ContactId, contact3.ContactId)
               .With(x => x.IsCustomerRelation, true)
               .With(x => x.ActionLevel, 3)
               .Create();

            var searchTerm = string.Empty;

            var request = new GetAssociatedContactsRequest
            {
                Search = searchTerm,
                Sorting = new Sorting
                {
                    Field = SortingConstants.EMAIL,
                    Descending = true
                },
                ContactType = ContactType.Collaborator
            };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(new List<ContactEntity>() { contact1, contact2, contact3 });
            await context.SaveChangesAsync();
            context.RoleEntity.AddRange(new List<RoleEntity>() { role1, role2, role3 });
            await context.SaveChangesAsync();
            var accountRepository = new AccountRepository(context);

            // Act
            var result = await accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);

            // Assert
            Assert.Equal(3, result.Items!.Count());
            Assert.Equal("Nadim", result.Items.ToList()[0].FirstName);
            Assert.Equal("Mostapha", result.Items.ToList()[1].FirstName);
            Assert.Equal("Awad", result.Items.ToList()[2].FirstName);
        }
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_WithSearch_ShouldFilterResultsOrderByCreationDateDescending()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            var accountId = 1;
            var contactId = 1;

            var account = _fixture.Build<AccountEntity>()
                    .With(x => x.AccountId, accountId)
                    .Without(x => x.RoleEntity)
                    .Without(x => x.PhoneEntity)
                    .Without(x => x.AddressEntity)
                    .Without(x => x.DeploymentEntity)
                    .Without(x => x.Office)
                    .Without(x => x.Delegation)
                    .Without(x => x.RoleLabelEntity)
                    .Without(x => x.OfferEligibilityEntity)
                    .Without(x => x.DematModalClosureEntity)
                    .With(x => x.IsActive, true)
                    .Create();

            var contact1 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Mostapha")
                .With(x => x.Email, "mostapha@abc.com")
                .With(x => x.CreationDate, DateTime.Now)
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, contactId)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var contact2 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Nadim")
                .With(x => x.Email, "nadim@abc.com")
                .With(x => x.CreationDate, DateTime.Now.AddDays(1))
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, 2)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .With(x => x.IsActive, true)
                .Create();

            var contact3 = _fixture.Build<ContactEntity>()
                .With(x => x.FirstName, "Awad")
                .With(x => x.Email, "awad@abc.com")
                .With(x => x.CreationDate, DateTime.Now.AddDays(2))
                .With(x => x.Type, ContactType.Collaborator.ToString())
                .With(x => x.ContactId, 3)
                .Without(x => x.RoleEntity)
                .Without(x => x.DelegationEntityDelegatee)
                .Without(x => x.DelegationEntityDelegator)
                .Without(x => x.RoleLabelEntityContact)
                .Without(x => x.RoleLabelEntityCreatedByNavigation)
                .Create();

            var role1 = _fixture.Build<RoleEntity>()
                .Without(x => x.Account)
                .Without(x => x.Contact)
                .With(x => x.AccountId, account.AccountId)
                .With(x => x.ContactId, contact1.ContactId)
                .With(x => x.IsCustomerRelation, true)
                .With(x => x.ActionLevel, 1)
                .Create();

            var role2 = _fixture.Build<RoleEntity>()
               .Without(x => x.Account)
               .Without(x => x.Contact)
               .With(x => x.AccountId, account.AccountId)
               .With(x => x.ContactId, contact2.ContactId)
               .With(x => x.IsCustomerRelation, true)
               .With(x => x.ActionLevel, 2)
               .Create();

            var role3 = _fixture.Build<RoleEntity>()
               .Without(x => x.Account)
               .Without(x => x.Contact)
               .With(x => x.AccountId, account.AccountId)
               .With(x => x.ContactId, contact3.ContactId)
               .With(x => x.IsCustomerRelation, true)
               .With(x => x.ActionLevel, 3)
               .Create();

            var searchTerm = string.Empty;

            var request = new GetAssociatedContactsRequest
            {
                Search = searchTerm,
                Sorting = new Sorting
                {
                    Field = SortingConstants.DATE,
                    Descending = true
                },
                ContactType = ContactType.Collaborator
            };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(new List<ContactEntity>() { contact1, contact2, contact3 });
            await context.SaveChangesAsync();
            context.RoleEntity.AddRange(new List<RoleEntity>() { role1, role2, role3 });
            await context.SaveChangesAsync();
            var accountRepository = new AccountRepository(context);

            // Act
            var result = await accountRepository.GetAssociatedContactsAsync(contactId, request, pagination);

            // Assert
            Assert.Equal(3, result.Items!.Count());
            Assert.Equal("Awad", result.Items.ToList()[0].FirstName);
            Assert.Equal("Nadim", result.Items.ToList()[1].FirstName);
            Assert.Equal("Mostapha", result.Items.ToList()[2].FirstName);
        }
    }

    [Fact]
    public async Task GetAccountsAsync_WithFilter_ShouldReturnPagingAccount()
    {
        using var context = new AccountContext(_dbContextOptions);

        var accounts = new List<AccountEntity>
        {
            new()
            {
                AccountId = 1,
                AccountGlobalUniqueId = Guid.NewGuid(),
                AccountNumber = "num1",
                CreatedBy = "moi",
                LegalName = "legal1",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
            },
            new()
            {
                AccountId = 2,
                AccountGlobalUniqueId = Guid.NewGuid(),
                LegalName = "legal2",
                AccountNumber = "num2",
                CreatedBy = "moi",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
            },
            new()
            {
                AccountId = 3,
                AccountGlobalUniqueId = Guid.NewGuid(),
                LegalName = "legal3",
                AccountNumber = "num3",
                CreatedBy = "moi",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
            },
            new()
            {
                AccountId = 4,
                AccountGlobalUniqueId = Guid.NewGuid(),
                LegalName = "legal4",
                AccountNumber = "num4",
                CreatedBy = "moi",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
            }
        };

        var roles = new List<RoleEntity>
        {
            new()
            {
                AccountId = 1,
                ContactId = 1,
                IsFavorite = true,
                IsCustomerRelation = true,
            },
            new()
            {
                AccountId = 2,
                ContactId = 1,
                IsFavorite = false,
                IsCustomerRelation = true,
            },
            new()
            {
                AccountId = 3,
                ContactId = 1,
                IsFavorite = true,
                IsCustomerRelation = false,
            },
            new()
            {
                AccountId = 4,
                ContactId = 2,
                IsFavorite = true,
                IsCustomerRelation = true,
            }
        };
        context.AccountEntity.AddRange(accounts);
        context.RoleEntity.AddRange(roles);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var criteria = new SearchAccountCriteria
        {
            ContactId = 1,
            IsFavoriteFilter = true,
            IsCustomerRelationFilter = true,
        };

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAccountsAsync_WithProspectAccount_ShouldExcludeProspectAccount()
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
            AccountNumber = "ACC-PROSPECT-001",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.RoleEntity.AddRange(
            new RoleEntity { Account = clientAccount, Contact = contact, ContactId = contact.ContactId, IsFavorite = true, IsCustomerRelation = true },
            new RoleEntity { Account = prospectAccount, Contact = contact, ContactId = contact.ContactId, IsFavorite = true, IsCustomerRelation = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountsAsync(
            new SearchAccountCriteria { ContactId = contact.ContactId },
            new Pagination { PageNumber = 1, PageSize = 10 });

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(clientAccount.AccountId, result.Items.Single().AccountId);
    }

    [Fact]
    public async Task GetAllAccountsAsync_WithProspectAccount_ShouldExcludeProspectAccount()
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
            AccountId = 10,
            AccountNumber = "ACC-CLIENT-010",
            LegalName = "Client Account",
            AccountType = "CLIENT",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var prospectAccount = new AccountEntity
        {
            AccountId = 20,
            AccountNumber = "ACC-PROSPECT-020",
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

        var repository = new AccountRepository(context);

        var result = await repository.GetAllAccountsAsync(
            null,
            new Pagination { PageNumber = 1, PageSize = 10 },
            new SearchAccountCriteria());

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(clientAccount.AccountId, result.Items.Single().AccountId);
    }

    [Theory]
    [InlineData(100, AccountType.CLIENT)]
    [InlineData(101, AccountType.PROSPECT)]
    public async Task GetAccountDetailAsync_WithSupportedAccountType_ShouldReturnAccountDetail(int accountId, AccountType accountType)
    {
        using var context = new AccountContext(_dbContextOptions);

        var account = new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = $"ACC-{accountType}-{accountId}",
            LegalName = $"{accountType} Account",
            AccountType = accountType.ToString(),
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountDetailAsync(accountId);

        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result!.AccountId);
        Assert.Equal(account.AccountNumber, result.AccountNumber);
        Assert.Equal(account.LegalName, result.Legal!.LegalName);
    }

    [Fact]
    public async Task GetAccountDetailAsync_WithInactiveProspectAccount_ShouldThrowNotFoundException()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 102,
            AccountNumber = "ACC-PROSPECT-102",
            LegalName = "Inactive Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = false,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        await Assert.ThrowsAsync<NotFoundException>(() => repository.GetAccountDetailAsync(prospectAccount.AccountId));
    }

    [Fact]
    public async Task GetAccountAsync_WithProspectAccount_ShouldReturnAccountDetail()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 101,
            AccountNumber = "ACC-PROSPECT-101",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountAsync(prospectAccount.AccountId);

        result.Should().NotBeNull();
        result.AccountId.Should().Be(prospectAccount.AccountId);
        result.AccountType.Should().Be(GlobalConstants.ProspectAccountType);
    }

    [Fact]
    public async Task GetAccountAsync_WithLowercaseProspectAccount_ShouldReturnAccountDetail()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 104,
            AccountNumber = "ACC-PROSPECT-104",
            LegalName = "Lowercase Prospect Account",
            AccountType = "prospect",
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountAsync(prospectAccount.AccountId);

        result.Should().NotBeNull();
        result.AccountId.Should().Be(prospectAccount.AccountId);
        result.AccountType.Should().Be("prospect");
    }

    [Fact]
    public async Task UpdateAccountAsync_WithProspectAccount_ShouldThrowNotFoundException()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 102,
            AccountNumber = "ACC-PROSPECT-102",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);
        var update = new AccountDetail
        {
            AccountNumber = "ACC-PROSPECT-102",
            Legal = new Legal
            {
                LegalName = "Updated Prospect",
                Siren = "123456789"
            },
            Phone = new List<Phone>()
        };

        await Assert.ThrowsAsync<NotFoundException>(() => repository.UpdateAccountAsync(prospectAccount.AccountId, update));
    }

    [Fact]
    public async Task UpdateAccountAsync_WithProspectAccount_AndIncludeProspects_ShouldUpdate()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 103,
            AccountNumber = "ACC-PROSPECT-103",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);
        var update = new AccountDetail
        {
            AccountNumber = "ACC-PROSPECT-103",
            Legal = new Legal
            {
                LegalName = "Updated Prospect",
                Siren = "123456789",
                StaffSizeRange = "10-50"
            },
            Phone = new List<Phone>()
        };

        var result = await repository.UpdateAccountAsync(prospectAccount.AccountId, update, includeProspects: true);

        result.Should().NotBeNull();
        result.AccountId.Should().Be(prospectAccount.AccountId);

        var persisted = await context.AccountEntity.IgnoreQueryFilters()
            .FirstAsync(a => a.AccountId == prospectAccount.AccountId);
        persisted.StaffSizeRange.Should().Be("10-50");
    }

    #region Prospect account filtering

    [Theory]
    [InlineData(103, AccountType.CLIENT)]
    [InlineData(104, AccountType.PROSPECT)]
    public async Task GetAccountSummaryAsync_WithSupportedAccountType_ShouldReturnAccountSummary(int accountId, AccountType accountType)
    {
        using var context = new AccountContext(_dbContextOptions);

        const int contactId = 1;
        var account = new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = $"ACC-{accountType}-{accountId}",
            LegalName = $"{accountType} Account",
            AccountType = accountType.ToString(),
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 },
            RoleEntity =
            [
                new RoleEntity
                {
                    AccountId = accountId,
                    ContactId = contactId,
                    IsSignatory = true
                }
            ]
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountSummaryAsync(contactId, accountId);

        Assert.NotNull(result);
        Assert.Equal(account.AccountId, result!.AccountId);
        Assert.Equal(account.AccountNumber, result.AccountNumber);
        Assert.Equal(account.LegalName, result.LegalName);
        Assert.Equal(result.AccountType, result.AccountType);
        Assert.True(result.IsSignatory);
    }

    [Fact]
    public async Task GetAccountSummaryAsync_WithInactiveProspectAccount_ShouldThrowNotFoundException()
    {
        using var context = new AccountContext(_dbContextOptions);

        var prospectAccount = new AccountEntity
        {
            AccountId = 105,
            AccountNumber = "ACC-PROSPECT-105",
            LegalName = "Inactive Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = false,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(prospectAccount);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        await Assert.ThrowsAsync<NotFoundException>(() => repository.GetAccountSummaryAsync(1, prospectAccount.AccountId));
    }

    [Theory]
    [InlineData("CLIENT")]
    [InlineData("PROSPECT")]
    public async Task GetContactsAccountAsync_WithSupportedAccountType_ShouldReturnContactsAccount(string accountType)
    {
        using var context = new AccountContext(_dbContextOptions);

        const int accountId = 106;
        const int contactId = 206;
        var contact = new ContactEntity
        {
            ContactId = contactId,
            Email = "contact@test.fr",
            FirstName = "Jean",
            LastName = "Dupont",
            PersonaName = "Jean Dupont",
            Type = ContactType.Collaborator.ToString(),
            CreationDate = DateTime.UtcNow,
            IsActive = true,
            RoleLabelEntityContact = [],
            RoleEntity =
            [
                new RoleEntity
                {
                    AccountId = accountId,
                    ContactId = contactId,
                    ActionLevel = 1,
                    IsCustomerRelation = true
                }
            ]
        };

        var account = new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = $"ACC-{accountType}-106",
            LegalName = $"{accountType} Account",
            AccountType = accountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.AccountEntity.Add(account);
        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);
        var criteria = new SearchContactsAccountCriteria();
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetContactsAccountAsync(accountId, criteria, pagination, includeProspects: true);

        Assert.Single(result.Items);
        Assert.Equal(contactId, result.Items.First().ContactId);
    }

    #region Contact widget contacts

    /// <summary>
    /// Ensures contact widget contacts include only collaborator roles labeled AM or CLP on the requested account.
    /// </summary>
    [Fact]
    public async Task GetAccountContactWidgetContactsAsync_ShouldReturnOnlyCollaboratorsWithAmOrClpLabels()
    {
        using var context = new AccountContext(_dbContextOptions);

        const int accountId = 306;
        const int otherAccountId = 307;
        var amContact = CreateContactWidgetContact(1, accountId, ContactType.Collaborator.ToString(), RoleLabelCodes.AccountManager);
        var clpContact = CreateContactWidgetContact(2, accountId, ContactType.Collaborator.ToString(), RoleLabelCodes.CustomerLeadPartner);
        var otherLabelContact = CreateContactWidgetContact(3, accountId, ContactType.Collaborator.ToString(), "OTHER");
        var customerContact = CreateContactWidgetContact(4, accountId, ContactType.Customer.ToString(), RoleLabelCodes.AccountManager);
        var otherAccountContact = CreateContactWidgetContact(5, otherAccountId, ContactType.Collaborator.ToString(), RoleLabelCodes.AccountManager);

        context.ContactEntity.AddRange(amContact, clpContact, otherLabelContact, customerContact, otherAccountContact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = (await repository.GetAccountContactWidgetContactsAsync(accountId)).ToList();

        Assert.Equal([amContact.ContactId, clpContact.ContactId], result.Select(contact => contact.ContactId));
        Assert.All(result, contact => Assert.Equal(ContactType.Collaborator.ToString(), contact.Type));
        Assert.Contains(result, contact => contact.Labels!.Any(label => label.Code == RoleLabelCodes.AccountManager));
        Assert.Contains(result, contact => contact.Labels!.Any(label => label.Code == RoleLabelCodes.CustomerLeadPartner));
        Assert.DoesNotContain(result, contact => contact.Labels!.Any(label => label.Code == "OTHER"));
    }

    /// <summary>
    /// Ensures contact widget contacts keep the existing Contact response model mapping.
    /// </summary>
    [Fact]
    public async Task GetAccountContactWidgetContactsAsync_ShouldKeepContactResponseShape()
    {
        using var context = new AccountContext(_dbContextOptions);

        const int accountId = 406;
        var contact = CreateContactWidgetContact(10, accountId, ContactType.Collaborator.ToString(), RoleLabelCodes.AccountManager);

        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = (await repository.GetAccountContactWidgetContactsAsync(accountId)).Single();

        Assert.Equal(contact.ContactId, result.ContactId);
        Assert.Equal(contact.ContactGlobalUniqueId, result.ContactGlobalUniqueId);
        Assert.Equal(contact.FirstName, result.FirstName);
        Assert.Equal(contact.LastName, result.LastName);
        Assert.Equal(contact.Email, result.Email);
        Assert.Equal(contact.MobilePhone, result.MobilePhone);
        Assert.Equal(contact.Type, result.Type);
        Assert.Equal(contact.Status, result.Status);
        Assert.Equal(contact.PersonaName, result.PersonaName);
        Assert.Equal(contact.Office, result.Office);
        Assert.Equal(contact.CreationDate, result.CreationDate);
        Assert.Equal(contact.IsActive, result.IsActive);
        Assert.Equal(contact.RoleEntity.First().IsCustomerRelation, result.IsCustomerRelation);
        Assert.Equal(contact.RoleEntity.First().ActionLevel, result.ActionLevel);
        Assert.Equal(contact.RoleEntity.First().ContactFlagPortailFactures, result.ContactFlagPortailFactures);
        Assert.Single(result.Labels!);
        Assert.Equal(RoleLabelCodes.AccountManager, result.Labels!.Single().Code);
    }

    /// <summary>
    /// Ensures contact widget contacts return an empty collection when no AM or CLP role label exists.
    /// </summary>
    [Fact]
    public async Task GetAccountContactWidgetContactsAsync_WhenNoAmOrClpLabelExists_ShouldReturnEmptyResult()
    {
        using var context = new AccountContext(_dbContextOptions);

        const int accountId = 506;
        var contact = CreateContactWidgetContact(20, accountId, ContactType.Collaborator.ToString(), "OTHER");

        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        var result = await repository.GetAccountContactWidgetContactsAsync(accountId);

        Assert.Empty(result);
    }

    /// <summary>
    /// Creates a contact with a role and one role label for contact widget repository tests.
    /// </summary>
    /// <param name="contactId">Contact identifier.</param>
    /// <param name="accountId">Account identifier.</param>
    /// <param name="contactType">Contact type.</param>
    /// <param name="labelCode">Role label code.</param>
    /// <returns>A contact entity configured with one account role and one role label.</returns>
    private static ContactEntity CreateContactWidgetContact(int contactId, int accountId, string contactType, string labelCode)
    {
        return new ContactEntity
        {
            ContactId = contactId,
            ContactGlobalUniqueId = Guid.NewGuid(),
            Email = $"contact-{contactId}@test.fr",
            FirstName = $"First{contactId}",
            LastName = $"Last{contactId}",
            MobilePhone = $"06000000{contactId:D2}",
            Type = contactType,
            Status = "Active",
            PersonaName = $"First{contactId} Last{contactId}",
            Office = "Office",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
            RoleEntity =
            [
                new RoleEntity
                {
                    AccountId = accountId,
                    ContactId = contactId,
                    ActionLevel = labelCode == RoleLabelCodes.CustomerLeadPartner ? 3 : 2,
                    IsCustomerRelation = true,
                    ContactFlagPortailFactures = true
                }
            ],
            RoleLabelEntityContact =
            [
                new RoleLabelEntity
                {
                    AccountId = accountId,
                    ContactId = contactId,
                    LabelId = contactId,
                    CreatedBy = contactId,
                    CreatedDate = DateTime.UtcNow,
                    Label = new LabelEntity
                    {
                        LabelId = contactId,
                        Code = labelCode,
                        CollaboratorLabel = $"Collaborator {labelCode}",
                        CustomerLabel = $"Customer {labelCode}",
                        Description = $"Description {labelCode}",
                        Business = "Transverse",
                        IsVisible = true
                    }
                }
            ]
        };
    }

    #endregion Contact widget contacts

    #endregion Prospect account filtering

    [Fact]
    public async Task GetAssociatedContactsAsync_WithOnlyProspectAccount_ShouldThrowNotFoundException()
    {
        using var context = new AccountContext(_dbContextOptions);

        var currentUser = new ContactEntity
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
        var associatedContact = new ContactEntity
        {
            ContactId = 2,
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
            AccountId = 104,
            AccountNumber = "ACC-PROSPECT-104",
            LegalName = "Prospect Account",
            AccountType = GlobalConstants.ProspectAccountType,
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };

        context.RoleEntity.AddRange(
            new RoleEntity { Account = prospectAccount, Contact = currentUser, ContactId = currentUser.ContactId, IsCustomerRelation = true },
            new RoleEntity { Account = prospectAccount, Contact = associatedContact, ContactId = associatedContact.ContactId, IsCustomerRelation = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new AccountRepository(context);

        await Assert.ThrowsAsync<NotFoundException>(() => repository.GetAssociatedContactsAsync(
            currentUser.ContactId,
            new GetAssociatedContactsRequest
            {
                ContactType = ContactType.Collaborator,
                Search = string.Empty
            },
            new Pagination { PageNumber = 1, PageSize = 10 }));
    }

    [Fact]
    public async Task CreateAccountAsync_WithValidRequest_ShouldCreateAndReturnAccountDetail()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "NEW001",
            LegalName = "New Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT
        };

        // Act
        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AccountId > 0);
        Assert.NotEqual(Guid.Empty, result.AccountGlobalUniqueId);
        Assert.Equal(request.AccountNumber, result.AccountNumber, ignoreCase: true);
    }

    [Fact]
    public async Task CreateAccountAsync_WithDuplicateAccountNumber_ShouldCreateBothAccounts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "DUP001",
            LegalName = "First Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT
        };

        await repository.CreateAccountAsync("creator@pulse.fr", request);
        await repository.CreateAccountAsync("creator@pulse.fr", request);

        var count = await context.AccountEntity.IgnoreQueryFilters().CountAsync(a => a.AccountNumber == request.AccountNumber.ToLower());
        Assert.Equal(2, count);
    }

    #region CreateAccountAsync coverage additions

    /// <summary>
    /// Ensures prospect account creation persists the expected prospect type and deployment metadata.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithProspectRequest_ShouldPersistProspectAccountTypeAndDeployment()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "PROSPECT001",
            LegalName = "Prospect Account",
            Siret = "12345678901234",
            AccountType = AccountType.PROSPECT
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);
        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .Include(account => account.DeploymentEntity)
            .FirstAsync(account => account.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        Assert.Equal(request.AccountType.ToString(), entity.AccountType);
        Assert.Equal(request.AccountNumber, entity.AccountNumber);
        Assert.Equal(request.LegalName, entity.LegalName);
        Assert.NotNull(entity.DeploymentEntity);
        Assert.Equal((int)DeploymentStatus.ToDeploy, entity.DeploymentEntity.Status);
        Assert.True(entity.IsActive);
    }

    #endregion CreateAccountAsync coverage additions

    #region CreateAccountAsync Address/LegalForm/NafCode coverage

    /// <summary>
    /// Ensures LegalForm, Address and a matching NafCode are persisted on the created account.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithLegalFormAddressAndValidNafCode_ShouldPersistAllFields()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);

        var naf = new NafEntity { NafCode = "62.01z", NafLabel = "programmation informatique" };
        context.NafEntity.Add(naf);
        await context.SaveChangesAsync();

        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "NAFOK001",
            LegalName = "Naf Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT,
            LegalForm = "sas",
            NafCode = "62.01z",
            Address = new AddressRequest
            {
                Street = "12 rue de la paix",
                City = "paris",
                Department = "paris",
                ZipCode = "75002",
                Region = "ile-de-france",
                Country = "france"
            }
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .Include(a => a.AddressEntity)
            .FirstAsync(a => a.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        Assert.Equal("SAS - SOCIÉTÉ PAR ACTIONS SIMPLIFIÉE", entity.LegalForm);
        Assert.Equal("SAS", entity.LegalFormCode);
        Assert.Equal(naf.NafId, entity.NafId);

        var address = Assert.Single(entity.AddressEntity);
        Assert.Equal(AddressType.Delivery.ToString(), address.AddressType);
        Assert.Equal(request.Address.Street, address.AddressLine1);
        Assert.Equal(request.Address.City, address.City);
        Assert.Equal(request.Address.Department, address.State);
        Assert.Equal(request.Address.ZipCode, address.ZipCode);
        Assert.Equal(request.Address.Country, address.Country);
    }

    /// <summary>
    /// Ensures a legal form received as a referential code (ex: "ENT IND" from the prospect flow)
    /// is persisted with the referential label in LegalForm and the code in LegalFormCode.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithLegalFormReferentialCode_ShouldPersistLabelAndCode()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "LEGALFORM001",
            LegalName = "Legal Form Code Account",
            Siret = "12345678901234",
            AccountType = AccountType.PROSPECT,
            LegalForm = "ENT IND"
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .FirstAsync(a => a.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        Assert.Equal("ENTREPRISE INDIVIDUELLE", entity.LegalForm);
        Assert.Equal("ENT IND", entity.LegalFormCode);
    }

    /// <summary>
    /// Ensures a legal form unknown from the referential is persisted as-is without code.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithUnknownLegalForm_ShouldPersistValueAsIsWithoutCode()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "LEGALFORM002",
            LegalName = "Unknown Legal Form Account",
            Siret = "12345678901234",
            AccountType = AccountType.PROSPECT,
            LegalForm = "ENTREPRISE INDIVIDUELLE"
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .FirstAsync(a => a.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        Assert.Equal("ENTREPRISE INDIVIDUELLE", entity.LegalForm);
        Assert.Null(entity.LegalFormCode);
    }

    /// <summary>
    /// Ensures the address Street is optional and does not prevent the address from being persisted.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithAddressWithoutStreet_ShouldPersistAddressWithNullAddressLine1()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "NOSTREET001",
            LegalName = "No Street Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT,
            Address = new AddressRequest
            {
                City = "paris",
                Department = "paris",
                Region = "ile-de-france",
                Country = "france"
            }
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .Include(a => a.AddressEntity)
            .FirstAsync(a => a.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        var address = Assert.Single(entity.AddressEntity);
        Assert.Null(address.AddressLine1);
        Assert.Null(address.ZipCode);
        Assert.Equal(request.Address.Department, address.State);
    }

    /// <summary>
    /// Ensures an unknown NafCode does not silently create the account without a NAF link.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithUnknownNafCode_ShouldThrowNotFoundException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "NAFKO001",
            LegalName = "Unknown Naf Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT,
            NafCode = "0000z"
        };

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => repository.CreateAccountAsync("creator@pulse.fr", request));

        Assert.Equal(Errors.NotFoundNafCode, exception.Code);
        Assert.DoesNotContain(await context.AccountEntity.IgnoreQueryFilters().ToListAsync(), a => a.AccountNumber == request.AccountNumber.ToLower());
    }

    /// <summary>
    /// Ensures NAF code with dot remains unchanged and still finds the correct record.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithNafCodeWithDot_ShouldFindCorrectNafWithoutModification()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);

        var naf = new NafEntity { NafCode = "52.10z", NafLabel = "Warehousing" };
        context.NafEntity.Add(naf);
        await context.SaveChangesAsync();

        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "NAFDOT001",
            LegalName = "Dot Naf Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT,
            NafCode = "52.10z" // With dot - should remain unchanged
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .FirstAsync(a => a.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        Assert.NotNull(entity);
        Assert.Equal(naf.NafId, entity.NafId);
        Assert.Equal(request.AccountNumber, entity.AccountNumber);
    }

    /// <summary>
    /// Ensures LegalForm, Address and NafCode remain optional and do not affect account creation when omitted.
    /// </summary>
    [Fact]
    public async Task CreateAccountAsync_WithoutLegalFormAddressOrNafCode_ShouldLeaveThemEmpty()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "NOOPT001",
            LegalName = "No Optional Fields Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT
        };

        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .Include(a => a.AddressEntity)
            .FirstAsync(a => a.AccountGlobalUniqueId == result.AccountGlobalUniqueId);

        Assert.Null(entity.LegalForm);
        Assert.Null(entity.NafId);
        Assert.Empty(entity.AddressEntity);
    }

    #endregion CreateAccountAsync Address/LegalForm/NafCode coverage

    [Fact]
    public async Task CreateAccountAsync_ShouldGenerateNewGuid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "GUID001",
            LegalName = "Guid Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT
        };

        // Act
        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.AccountGlobalUniqueId);

        var entity = await context.AccountEntity.IgnoreQueryFilters().FirstAsync(a => a.AccountNumber == request.AccountNumber.ToLower());
        Assert.Equal(result.AccountGlobalUniqueId, entity.AccountGlobalUniqueId);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldSetDeploymentStatusToToDeploy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "DEPLOY001",
            LegalName = "Deploy Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT
        };

        // Act
        var result = await repository.CreateAccountAsync("creator@pulse.fr", request);

        // Assert
        var entity = await context.AccountEntity
            .IgnoreQueryFilters()
            .Include(a => a.DeploymentEntity)
            .FirstAsync(a => a.AccountNumber == request.AccountNumber.ToLower());
        Assert.NotNull(entity.DeploymentEntity);
        Assert.Equal((int)DeploymentStatus.ToDeploy, entity.DeploymentEntity.Status);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldSetCreatedByAndCreationDate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var request = new CreateAccountRequest
        {
            AccountNumber = "META001",
            LegalName = "Meta Account",
            Siret = "12345678901234",
            AccountType = AccountType.CLIENT
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        await repository.CreateAccountAsync("creator@pulse.fr", request);

        // Assert
        var entity = await context.AccountEntity.IgnoreQueryFilters().FirstAsync(a => a.AccountNumber == request.AccountNumber.ToLower());
        Assert.Equal("creator@pulse.fr", entity.CreatedBy);
        Assert.True(entity.CreationDate >= beforeCreate);
        Assert.True(entity.IsActive);
    }

    [Fact]
    public async Task SearchAccountsAsync_WithLegalNameMatch_ShouldReturnResults()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var contact = new ContactEntity
        {
            Type = "1",
            FirstName = "John",
            LastName = "Director",
            Email = "director@acme.fr",
            CreationDate = DateTime.UtcNow,
            PersonaName = "director",
            IsActive = true,
        };

        var account = new AccountEntity
        {
            AccountNumber = "ACC00123",
            LegalName = "ACME Corporation",
            Siret = "12345678901234",
            CreatedBy = "test@pulse.fr",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString(),
            RoleEntity = new List<RoleEntity>
            {
                new()
                {
                    IsSignatory = true,
                    Contact = contact,
                }
            },
            DeploymentEntity = new DeploymentEntity
            {
                Status = (int)DeploymentStatus.ToDeploy,
                DeploymentDate = DateTime.UtcNow,
            }
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.SearchAccountsAsync("acme", pagination);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalItems.Should().Be(1);
        var firstItem = result.Items.First();
        firstItem.AccountId.Should().Be(account.AccountId);
        firstItem.LegalName.Should().Be("acme corporation");
        firstItem.AccountNumber.Should().Be("acc00123");
        firstItem.Siret.Should().Be("12345678901234");
        firstItem.DirectorEmail.Should().Be("director@acme.fr");
    }

    [Fact]
    public async Task SearchAccountsAsync_WithAccountNumberMatch_ShouldReturnResults()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var contact = new ContactEntity
        {
            Type = "1",
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.fr",
            CreationDate = DateTime.UtcNow,
            PersonaName = "jane",
            IsActive = true,
        };

        var account = new AccountEntity
        {
            AccountNumber = "TEST456",
            LegalName = "Test Company",
            Siret = "98765432109876",
            CreatedBy = "test@pulse.fr",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString(),
            RoleEntity = new List<RoleEntity>
            {
                new()
                {
                    IsSignatory = true,
                    Contact = contact,
                }
            },
            DeploymentEntity = new DeploymentEntity
            {
                Status = (int)DeploymentStatus.ToDeploy,
                DeploymentDate = DateTime.UtcNow,
            }
        };

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();

        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.SearchAccountsAsync("test456", pagination);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().AccountNumber.Should().Be("test456");
    }

    [Fact]
    public async Task SearchAccountsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var sharedContact = new ContactEntity
        {
            Type = "1",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.fr",
            CreationDate = DateTime.UtcNow,
            PersonaName = "test",
            IsActive = true,
        };

        for (int i = 1; i <= 25; i++)
        {
            var account = new AccountEntity
            {
                AccountNumber = $"ACC{i:D5}",
                LegalName = $"Company {i}",
                Siret = $"{i:D14}",
                CreatedBy = "test@pulse.fr",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                RoleEntity = new List<RoleEntity>
                {
                    new()
                    {
                        IsSignatory = true,
                        Contact = sharedContact,
                    }
                },
                DeploymentEntity = new DeploymentEntity
                {
                    Status = (int)DeploymentStatus.ToDeploy,
                    DeploymentDate = DateTime.UtcNow,
                }
            };

            context.AccountEntity.Add(account);
        }

        await context.SaveChangesAsync();

        var pagination = new Pagination { PageNumber = 2, PageSize = 10 };

        // Act
        var result = await repository.SearchAccountsAsync(null, pagination);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalItems.Should().Be(25);
        result.TotalPage.Should().Be(3);
        result.CurrentPage.Should().Be(2);
    }

    [Fact]
    public async Task SearchAccountsAsync_ShouldReturnSortedByLegalName()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var sharedContact = new ContactEntity
        {
            Type = "1",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.fr",
            CreationDate = DateTime.UtcNow,
            PersonaName = "test",
            IsActive = true,
        };

        var accounts = new[] { "Zebra Corp", "Apple Inc", "Microsoft Ltd" };
        foreach (var name in accounts)
        {
            var account = new AccountEntity
            {
                AccountNumber = Guid.NewGuid().ToString(),
                LegalName = name,
                Siret = "12345678901234",
                CreatedBy = "test@pulse.fr",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                RoleEntity = new List<RoleEntity>
                {
                    new()
                    {
                        IsSignatory = true,
                        Contact = sharedContact,
                    }
                },
                DeploymentEntity = new DeploymentEntity
                {
                    Status = (int)DeploymentStatus.ToDeploy,
                    DeploymentDate = DateTime.UtcNow,
                }
            };

            context.AccountEntity.Add(account);
        }

        await context.SaveChangesAsync();

        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.SearchAccountsAsync(null, pagination);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        var itemsList = result.Items.ToList();
        itemsList[0].LegalName.Should().Be("apple inc");
        itemsList[1].LegalName.Should().Be("microsoft ltd");
        itemsList[2].LegalName.Should().Be("zebra corp");
    }

    [Fact]
    public async Task SearchAccountsAsync_WithNoQuery_ShouldReturnAllAccounts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAccountContext(options);
        var repository = new AccountRepository(context);

        var sharedContact = new ContactEntity
        {
            Type = "1",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.fr",
            CreationDate = DateTime.UtcNow,
            PersonaName = "test",
            IsActive = true,
        };

        for (int i = 1; i <= 5; i++)
        {
            var account = new AccountEntity
            {
                AccountNumber = $"ACC{i}",
                LegalName = $"Company {i}",
                Siret = $"{i:D14}",
                CreatedBy = "test@pulse.fr",
                IsActive = true,
                AccountType = AccountType.CLIENT.ToString(),
                RoleEntity = new List<RoleEntity>
                {
                    new()
                    {
                        IsSignatory = true,
                        Contact = sharedContact,
                    }
                },
                DeploymentEntity = new DeploymentEntity
                {
                    Status = (int)DeploymentStatus.ToDeploy,
                    DeploymentDate = DateTime.UtcNow,
                }
            };

            context.AccountEntity.Add(account);
        }

        await context.SaveChangesAsync();

        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await repository.SearchAccountsAsync(null, pagination);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalItems.Should().Be(5);
    }

    #region IsContactProspectOnlyAsync

    /// <summary>
    /// Ensures the prospect-only check returns true when the contact has only prospect account roles.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WithOnlyProspectRoles_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var contact = CreateProspectOnlyContact(100);
        var firstProspectAccount = CreateProspectOnlyAccount(200, GlobalConstants.ProspectAccountType);
        var secondProspectAccount = CreateProspectOnlyAccount(201, AccountType.PROSPECT.ToString());

        context.RoleEntity.AddRange(
            CreateProspectOnlyRole(contact, firstProspectAccount),
            CreateProspectOnlyRole(contact, secondProspectAccount));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await repository.IsContactProspectOnlyAsync(contact.ContactId);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Ensures the prospect-only check returns false when the contact has a non-prospect account role.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WithNonProspectRole_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var contact = CreateProspectOnlyContact(101);
        var clientAccount = CreateProspectOnlyAccount(202, AccountType.CLIENT.ToString());

        context.RoleEntity.Add(CreateProspectOnlyRole(contact, clientAccount));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await repository.IsContactProspectOnlyAsync(contact.ContactId);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Ensures the prospect-only check returns false when the contact has both prospect and non-prospect roles.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WithMixedProspectAndNonProspectRoles_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var contact = CreateProspectOnlyContact(102);
        var prospectAccount = CreateProspectOnlyAccount(203, GlobalConstants.ProspectAccountType);
        var clientAccount = CreateProspectOnlyAccount(204, AccountType.CLIENT.ToString());

        context.RoleEntity.AddRange(
            CreateProspectOnlyRole(contact, prospectAccount),
            CreateProspectOnlyRole(contact, clientAccount));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await repository.IsContactProspectOnlyAsync(contact.ContactId);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Ensures the prospect-only check returns false for an existing contact without roles.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WithNoRoles_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var contact = CreateProspectOnlyContact(103);

        context.ContactEntity.Add(contact);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await repository.IsContactProspectOnlyAsync(contact.ContactId);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Ensures the prospect-only check throws the existing contact-not-found exception when the contact does not exist.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WhenContactDoesNotExist_ShouldThrowNotFoundException()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var contactId = 104;

        var result = async () => await repository.IsContactProspectOnlyAsync(contactId);

        var exception = await Assert.ThrowsAsync<NotFoundException>(result);
        exception.Code.Should().Be(Errors.NotFoundContactCode);
        exception.Message.Should().Be(string.Format(Errors.NotFoundContactMessage, contactId));
    }

    /// <summary>
    /// Ensures contact existence is checked before evaluating roles for the requested identifier.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WhenRoleExistsButContactDoesNotExist_ShouldThrowNotFoundException()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var contactId = 105;
        var prospectAccount = CreateProspectOnlyAccount(205, GlobalConstants.ProspectAccountType);

        context.RoleEntity.Add(new RoleEntity
        {
            ContactId = contactId,
            Account = prospectAccount,
            AccountId = prospectAccount.AccountId
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = async () => await repository.IsContactProspectOnlyAsync(contactId);

        var exception = await Assert.ThrowsAsync<NotFoundException>(result);
        exception.Code.Should().Be(Errors.NotFoundContactCode);
        exception.Message.Should().Be(string.Format(Errors.NotFoundContactMessage, contactId));
    }

    /// <summary>
    /// Ensures the prospect-only check is scoped to the requested contact identifier.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WhenAnotherContactHasClientRole_ShouldReturnTrueForProspectOnlyContact()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new AccountRepository(context);
        var prospectOnlyContact = CreateProspectOnlyContact(106);
        var otherContact = CreateProspectOnlyContact(107);
        var prospectAccount = CreateProspectOnlyAccount(206, GlobalConstants.ProspectAccountType);
        var clientAccount = CreateProspectOnlyAccount(207, AccountType.CLIENT.ToString());

        context.RoleEntity.AddRange(
            CreateProspectOnlyRole(prospectOnlyContact, prospectAccount),
            CreateProspectOnlyRole(otherContact, clientAccount));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await repository.IsContactProspectOnlyAsync(prospectOnlyContact.ContactId);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Creates a contact entity for prospect-only repository tests.
    /// </summary>
    /// <param name="contactId">The contact identifier.</param>
    /// <returns>A contact entity.</returns>
    private static ContactEntity CreateProspectOnlyContact(int contactId)
    {
        return new ContactEntity
        {
            ContactId = contactId,
            Type = ContactType.Collaborator.ToString(),
            FirstName = $"First {contactId}",
            LastName = $"Last {contactId}",
            Email = $"contact{contactId}@test.fr",
            PersonaName = $"Contact {contactId}",
            CreationDate = DateTime.UtcNow,
            IsActive = true,
        };
    }

    /// <summary>
    /// Creates an account entity for prospect-only repository tests.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="accountType">The account type.</param>
    /// <returns>An account entity.</returns>
    private static AccountEntity CreateProspectOnlyAccount(int accountId, string accountType)
    {
        return new AccountEntity
        {
            AccountId = accountId,
            AccountNumber = $"ACC-{accountId}",
            LegalName = $"Account {accountId}",
            AccountType = accountType,
            CreatedBy = "tests",
            IsActive = true,
        };
    }

    /// <summary>
    /// Creates a role entity linking a contact to an account for prospect-only repository tests.
    /// </summary>
    /// <param name="contact">The contact entity.</param>
    /// <param name="account">The account entity.</param>
    /// <returns>A role entity.</returns>
    private static RoleEntity CreateProspectOnlyRole(ContactEntity contact, AccountEntity account)
    {
        return new RoleEntity
        {
            Contact = contact,
            ContactId = contact.ContactId,
            Account = account,
            AccountId = account.AccountId
        };
    }

    #endregion IsContactProspectOnlyAsync

    #region Prospect filtering regression coverage additions

    /// <summary>
    /// Ensures account-number filtering in the global account list still excludes prospects.
    /// </summary>
    [Fact]
    public async Task GetAllAccountsAsync_WithAccountNumberFilter_ShouldReturnMatchingClientAndExcludeMatchingProspect()
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
            AccountId = 200,
            AccountNumber = "ACC-SHARED-CLIENT",
            LegalName = "Client Account",
            AccountType = AccountType.CLIENT.ToString(),
            CreatedBy = "tests",
            IsActive = true,
            DeploymentEntity = new DeploymentEntity { Status = 1 }
        };
        var prospectAccount = new AccountEntity
        {
            AccountId = 201,
            AccountNumber = "ACC-SHARED-PROSPECT",
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

        var repository = new AccountRepository(context);

        var result = await repository.GetAllAccountsAsync(
            "ACC-SHARED",
            new Pagination { PageNumber = 1, PageSize = 10 },
            new SearchAccountCriteria());

        Assert.Single(result.Items);
        Assert.Equal(clientAccount.AccountId, result.Items.Single().AccountId);
        Assert.Equal(clientAccount.AccountNumber, result.Items.Single().AccountNumber);
    }

    #endregion Prospect filtering regression coverage additions

    #region LABEL sorting coverage

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetContactsAccountAsync_LabelSorting_Collab_SortsByHasLabel(bool descending)
    {
        using var context = new AccountContext(_dbContextOptions);

        var accountId = 50;
        var account = new AccountEntity { AccountId = accountId, AccountNumber = "ACC-50", LegalName = "Test", AccountType = "CLIENT", CreatedBy = "tests", IsActive = true };

        var labelAm = new LabelEntity { LabelId = 10, Code = "AM", Business = "ESG", IsVisible = true, CollaboratorLabel = "AM", CustomerLabel = "AM" };

        var contactWithLabel = new ContactEntity
        {
            ContactId = 101,
            Email = "with@label.fr",
            FirstName = "WithLabel",
            LastName = "A",
            PersonaName = "WithLabel A",
            Type = ContactType.Collaborator.ToString(),
            IsActive = true,
            RoleLabelEntityContact = new List<RoleLabelEntity> { new() { AccountId = accountId, ContactId = 101, LabelId = 10 } },
            RoleEntity = new List<RoleEntity> { new() { AccountId = accountId, ContactId = 101 } }
        };
        var contactWithoutLabel = new ContactEntity
        {
            ContactId = 102,
            Email = "without@label.fr",
            FirstName = "WithoutLabel",
            LastName = "B",
            PersonaName = "WithoutLabel B",
            Type = ContactType.Collaborator.ToString(),
            IsActive = true,
            RoleLabelEntityContact = new List<RoleLabelEntity>(),
            RoleEntity = new List<RoleEntity> { new() { AccountId = accountId, ContactId = 102 } }
        };

        context.AccountEntity.Add(account);
        context.LabelEntity.Add(labelAm);
        context.ContactEntity.AddRange(contactWithLabel, contactWithoutLabel);
        await context.SaveChangesAsync();

        var criteria = new SearchContactsAccountCriteria
        {
            Type = ContactType.Collaborator,
            Sorting = new Sorting { Field = SortingConstants.LABEL, Descending = descending }
        };
        var repository = new AccountRepository(context);

        var result = await repository.GetContactsAccountAsync(accountId, criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Should().HaveCount(2);
        if (descending)
        {
            result.Items.First().ContactId.Should().Be(101);
        }
        else
        {
            result.Items.First().ContactId.Should().Be(102);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetContactsAccountAsync_LabelSorting_Customer_SortsByIsSignatoryThenFlagPortail(bool descending)
    {
        using var context = new AccountContext(_dbContextOptions);

        var accountId = 60;
        var account = new AccountEntity { AccountId = accountId, AccountNumber = "ACC-60", LegalName = "Test", AccountType = "CLIENT", CreatedBy = "tests", IsActive = true };

        var contactSignatory = new ContactEntity
        {
            ContactId = 201,
            Email = "signatory@test.fr",
            FirstName = "Signatory",
            LastName = "A",
            PersonaName = "Signatory A",
            Type = ContactType.Customer.ToString(),
            IsActive = true,
            RoleLabelEntityContact = new List<RoleLabelEntity>(),
            RoleEntity = new List<RoleEntity> { new() { AccountId = accountId, ContactId = 201, IsSignatory = true, ContactFlagPortailFactures = false } }
        };
        var contactNonSignatory = new ContactEntity
        {
            ContactId = 202,
            Email = "nonsignatory@test.fr",
            FirstName = "NonSignatory",
            LastName = "B",
            PersonaName = "NonSignatory B",
            Type = ContactType.Customer.ToString(),
            IsActive = true,
            RoleLabelEntityContact = new List<RoleLabelEntity>(),
            RoleEntity = new List<RoleEntity> { new() { AccountId = accountId, ContactId = 202, IsSignatory = false, ContactFlagPortailFactures = false } }
        };

        context.AccountEntity.Add(account);
        context.ContactEntity.AddRange(contactSignatory, contactNonSignatory);
        await context.SaveChangesAsync();

        var criteria = new SearchContactsAccountCriteria
        {
            Type = ContactType.Customer,
            Sorting = new Sorting { Field = SortingConstants.LABEL, Descending = descending }
        };
        var repository = new AccountRepository(context);

        var result = await repository.GetContactsAccountAsync(accountId, criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Should().HaveCount(2);
        if (descending)
        {
            result.Items.First().ContactId.Should().Be(201);
        }
        else
        {
            result.Items.First().ContactId.Should().Be(202);
        }
    }

    #endregion LABEL sorting coverage

    #region LastActivityDate filter

    private static readonly DateTime ActivityReference = new(2026, 7, 6, 12, 0, 0, DateTimeKind.Utc);

    private static ContactEntity CreateActivityContact(string email, int contactId)
    {
        return new ContactEntity
        {
            ContactId = contactId,
            Type = "1",
            FirstName = "first",
            LastName = "last",
            Email = email,
            CreationDate = DateTime.Now,
            PersonaName = "persona",
            IsActive = true,
        };
    }

    private static AccountEntity CreateAccountWithActivity(ContactEntity contact, string legalName, DateTime? lastActivityDate, bool isFavorite = false)
    {
        var account = new AccountEntity
        {
            AccountNumber = legalName,
            LegalName = legalName,
            CreatedBy = "me",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString(),
            RoleEntity = new List<RoleEntity>
            {
                new()
                {
                    Contact = contact,
                    LastActivityDate = lastActivityDate,
                    IsFavorite = isFavorite,
                }
            },
        };

        account.DeploymentEntity = new DeploymentEntity { Account = account, Status = 1 };
        return account;
    }

    [Fact]
    public async Task GetAccountsAsync_WhenLastActivityDateFromIsSet_ShouldReturnOnlyAccountsUsedSinceAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "recent", ActivityReference.AddDays(-5)),
            CreateAccountWithActivity(contact, "boundary", ActivityReference.AddDays(-30)),
            CreateAccountWithActivity(contact, "old", ActivityReference.AddDays(-40)),
            CreateAccountWithActivity(contact, "never", null));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            LastActivityDateFrom = ActivityReference.AddDays(-30),
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName).Should().BeEquivalentTo("recent", "boundary");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenNoLastActivityRange_ShouldReturnAllAccountsAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "recent", ActivityReference.AddDays(-5)),
            CreateAccountWithActivity(contact, "old", ActivityReference.AddDays(-40)),
            CreateAccountWithActivity(contact, "never", null));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAccountsAsync_WhenLastActivityDateToIsSet_ShouldExcludeAccountsUsedAfterAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "recent", ActivityReference.AddDays(-5)),
            CreateAccountWithActivity(contact, "mid", ActivityReference.AddDays(-20)),
            CreateAccountWithActivity(contact, "old", ActivityReference.AddDays(-40)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            LastActivityDateFrom = ActivityReference.AddDays(-30),
            LastActivityDateTo = ActivityReference.AddDays(-10),
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName).Should().BeEquivalentTo("mid");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenLastActivityDateEqualsUpperBound_ShouldIncludeAccountAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "on upper bound", ActivityReference.AddDays(-10)),
            CreateAccountWithActivity(contact, "after upper bound", ActivityReference.AddDays(-5)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            LastActivityDateTo = ActivityReference.AddDays(-10),
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName).Should().BeEquivalentTo("on upper bound");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenOtherContactHasRecentActivity_ShouldNotMatchCurrentContactFilterAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var currentContact = CreateActivityContact("current@test.fr", 501);
        var otherContact = CreateActivityContact("other@test.fr", 502);
        var account = CreateAccountWithActivity(currentContact, "shared", null);
        account.RoleEntity.Add(new RoleEntity
        {
            Contact = otherContact,
            LastActivityDate = ActivityReference.AddDays(-1),
        });
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = currentContact.ContactId,
            LastActivityDateFrom = ActivityReference.AddDays(-30),
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccountsAsync_WhenLastActivityRangeAndFavoriteFilter_ShouldApplyBothAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "favorite recent", ActivityReference.AddDays(-5), isFavorite: true),
            CreateAccountWithActivity(contact, "favorite old", ActivityReference.AddDays(-40), isFavorite: true),
            CreateAccountWithActivity(contact, "not favorite recent", ActivityReference.AddDays(-5)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            IsFavoriteFilter = true,
            LastActivityDateFrom = ActivityReference.AddDays(-30),
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName).Should().BeEquivalentTo("favorite recent");
    }

    #endregion LastActivityDate filter

    #region Default portfolio sort

    [Fact]
    public async Task GetAccountsAsync_WhenNoSorting_ShouldReturnStandardPortfolioOrderAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "reg recent", ActivityReference.AddDays(-2)),
            CreateAccountWithActivity(contact, "fav old", ActivityReference.AddDays(-40), isFavorite: true),
            CreateAccountWithActivity(contact, "reg none", null),
            CreateAccountWithActivity(contact, "fav recent", ActivityReference.AddDays(-1), isFavorite: true),
            CreateAccountWithActivity(contact, "reg old", ActivityReference.AddDays(-50)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName)
            .Should().Equal("fav recent", "reg recent", "fav old", "reg old", "reg none");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenNoSortingAndLastActivityDisabled_ShouldSortAlphabeticallyAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "zzz fav recent", ActivityReference.AddDays(-1), isFavorite: true),
            CreateAccountWithActivity(contact, "mmm reg", ActivityReference.AddDays(-5)),
            CreateAccountWithActivity(contact, "aaa reg none", null));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
        };

        var result = await repository.GetAccountsAsync(
            criteria,
            new Pagination { PageNumber = 1, PageSize = 10 },
            sortByLastActivity: false);

        result.Items.Select(account => account.LegalName)
            .Should().Equal("aaa reg none", "mmm reg", "zzz fav recent");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenSortingByCompanyName_ShouldNotPutFavoritesFirstAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "zzz favorite", ActivityReference.AddDays(-1), isFavorite: true),
            CreateAccountWithActivity(contact, "aaa regular", ActivityReference.AddDays(-40)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            Sorting = new Sorting { Field = SortingConstants.COMPANYNAME, Descending = false },
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName)
            .Should().Equal("aaa regular", "zzz favorite");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenExplicitLastActivitySort_ShouldSortByActivityAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "aaa old", ActivityReference.AddDays(-40)),
            CreateAccountWithActivity(contact, "zzz recent", ActivityReference.AddDays(-1)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            Sorting = new Sorting { Field = SortingConstants.LASTACTIVITYDATE, Descending = true },
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName)
            .Should().Equal("zzz recent", "aaa old");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenSortingByIsFavoriteDescending_ShouldReturnFavoritesFirstThenLastActivityAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "aaa reg recent", ActivityReference.AddDays(-1)),
            CreateAccountWithActivity(contact, "zzz reg old", ActivityReference.AddDays(-60)),
            CreateAccountWithActivity(contact, "zzz fav recent", ActivityReference.AddDays(-40), isFavorite: true),
            CreateAccountWithActivity(contact, "bbb fav old", ActivityReference.AddDays(-50), isFavorite: true));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            Sorting = new Sorting { Field = SortingConstants.ISFAVORITE, Descending = true },
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName)
            .Should().Equal("zzz fav recent", "bbb fav old", "aaa reg recent", "zzz reg old");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenSortingByIsFavoriteAscending_ShouldReturnFavoritesLastAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "zzz fav", ActivityReference.AddDays(-1), isFavorite: true),
            CreateAccountWithActivity(contact, "aaa reg old", ActivityReference.AddDays(-40)),
            CreateAccountWithActivity(contact, "bbb reg recent", ActivityReference.AddDays(-2)));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            Sorting = new Sorting { Field = SortingConstants.ISFAVORITE, Descending = false },
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName)
            .Should().Equal("bbb reg recent", "aaa reg old", "zzz fav");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenSortingByIsFavoriteAndLastActivityDisabled_ShouldFallBackOnLegalNameAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        context.AccountEntity.AddRange(
            CreateAccountWithActivity(contact, "aaa reg recent", ActivityReference.AddDays(-1)),
            CreateAccountWithActivity(contact, "zzz fav recent", ActivityReference.AddDays(-2), isFavorite: true),
            CreateAccountWithActivity(contact, "bbb fav old", ActivityReference.AddDays(-50), isFavorite: true));
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            Sorting = new Sorting { Field = SortingConstants.ISFAVORITE, Descending = true },
        };

        var result = await repository.GetAccountsAsync(
            criteria,
            new Pagination { PageNumber = 1, PageSize = 10 },
            sortByLastActivity: false);

        result.Items.Select(account => account.LegalName)
            .Should().Equal("bbb fav old", "zzz fav recent", "aaa reg recent");
    }

    #endregion Default portfolio sort

    #region Search signatory scope

    [Fact]
    public async Task GetAccountsAsync_WhenSignatoryMatchesOnForeignAccount_ShouldNotLeakOutsidePortfolioAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var currentContact = CreateActivityContact("me@test.fr", 501);
        var signatory = CreateActivityContact("bob.signataire@test.fr", 502);
        signatory.FirstName = "bob";
        signatory.LastName = "signataire";
        var otherOwner = CreateActivityContact("other@test.fr", 503);

        var mine = CreateAccountWithActivity(currentContact, "mine", null);
        mine.RoleEntity.Add(new RoleEntity { Contact = signatory, IsSignatory = true });
        var foreign = CreateAccountWithActivity(otherOwner, "foreign", null);
        foreign.RoleEntity.Add(new RoleEntity { Contact = signatory, IsSignatory = true });

        context.AccountEntity.AddRange(mine, foreign);
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = currentContact.ContactId,
            Search = "bob",
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.LegalName).Should().BeEquivalentTo("mine");
    }

    [Fact]
    public async Task GetAccountsAsync_WhenAccountMatchesNameAndSignatory_ShouldReturnItOnceAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var currentContact = CreateActivityContact("me@test.fr", 501);
        var signatory = CreateActivityContact("bob.signataire@test.fr", 502);
        signatory.FirstName = "bob";
        signatory.LastName = "signataire";

        var account = CreateAccountWithActivity(currentContact, "bob and co", null);
        account.RoleEntity.Add(new RoleEntity { Contact = signatory, IsSignatory = true });

        context.AccountEntity.Add(account);
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = currentContact.ContactId,
            Search = "bob",
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Should().ContainSingle(a => a.LegalName == "bob and co");
        result.TotalItems.Should().Be(1);
    }

    #endregion Search signatory scope

    #region Deterministic paging order

    [Fact]
    public async Task GetAccountsAsync_WhenSortKeysTie_ShouldOrderByAccountIdAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        var second = CreateAccountWithActivity(contact, "twin", null);
        second.AccountId = 902;
        var first = CreateAccountWithActivity(contact, "twin", null);
        first.AccountId = 901;
        context.AccountEntity.AddRange(second, first);
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.AccountId).Should().Equal(901, 902);
    }

    [Fact]
    public async Task GetAccountsAsync_WhenSortingByStatusWithTies_ShouldOrderByAccountIdAsync()
    {
        using var context = new TestAccountContext(_dbContextOptions);
        var contact = CreateActivityContact("current@test.fr", 500);
        var second = CreateAccountWithActivity(contact, "beta", null);
        second.AccountId = 902;
        var first = CreateAccountWithActivity(contact, "alpha", null);
        first.AccountId = 901;
        context.AccountEntity.AddRange(second, first);
        await context.SaveChangesAsync();
        var repository = new AccountRepository(context);
        var criteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId,
            Sorting = new Sorting { Field = SortingConstants.STATUS, Descending = false },
        };

        var result = await repository.GetAccountsAsync(criteria, new Pagination { PageNumber = 1, PageSize = 10 });

        result.Items.Select(account => account.AccountId).Should().Equal(901, 902);
    }

    #endregion Deterministic paging order

    #region Reset demat email

    /// <summary>
    /// Verifies that only the target account's dematerialization email is cleared.
    /// </summary>
    [Fact]
    public async Task ResetDematEmailAsync_WhenAccountExists_ShouldClearOnlyTargetEmailAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var context = CreateDematSqliteContext();
        var target = CreateDematAccount(901, "target@test.fr", "delivery@test.fr", "billing@test.fr");
        var other = CreateDematAccount(902, "other@test.fr", "other-delivery@test.fr", "other-billing@test.fr");
        context.AccountEntity.AddRange(target, other);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();
        var repository = new AccountRepository(context);

        var result = await repository.ResetDematEmailAsync(target.AccountId);

        var persistedTarget = await context.AccountEntity.SingleAsync(account => account.AccountId == target.AccountId, cancellationToken);
        var persistedOther = await context.AccountEntity.SingleAsync(account => account.AccountId == other.AccountId, cancellationToken);
        result.Should().BeTrue();
        persistedTarget.Email.Should().BeNull();
        persistedTarget.DeliveryEmail.Should().Be("delivery@test.fr");
        persistedTarget.BillingEmail.Should().Be("billing@test.fr");
        persistedOther.Email.Should().Be("other@test.fr");
    }

    /// <summary>
    /// Verifies that resetting an already empty email is idempotent.
    /// </summary>
    [Fact]
    public async Task ResetDematEmailAsync_WhenEmailIsAlreadyNull_ShouldRemainSuccessfulAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var context = CreateDematSqliteContext();
        var account = CreateDematAccount(901, null!, "delivery@test.fr", "billing@test.fr");
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();
        var repository = new AccountRepository(context);

        var firstResult = await repository.ResetDematEmailAsync(account.AccountId);
        var secondResult = await repository.ResetDematEmailAsync(account.AccountId);

        firstResult.Should().BeTrue();
        secondResult.Should().BeTrue();
        var persistedAccount = await context.AccountEntity.SingleAsync(entity => entity.AccountId == account.AccountId, cancellationToken);
        persistedAccount.Email.Should().BeNull();
        persistedAccount.DeliveryEmail.Should().Be("delivery@test.fr");
        persistedAccount.BillingEmail.Should().Be("billing@test.fr");
    }

    /// <summary>
    /// Verifies that resetting an unknown account reports that it was not found.
    /// </summary>
    [Fact]
    public async Task ResetDematEmailAsync_WhenAccountDoesNotExist_ShouldReturnFalseAsync()
    {
        using var context = CreateDematSqliteContext();
        var repository = new AccountRepository(context);

        var result = await repository.ResetDematEmailAsync(901);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that accounts excluded by the standard Account query filters are not reset.
    /// </summary>
    /// <param name="isActive">Whether the account is active.</param>
    /// <param name="accountType">The account type.</param>
    [Theory]
    [InlineData(false, "CLIENT")]
    [InlineData(true, "PROSPECT")]
    public async Task ResetDematEmailAsync_WhenAccountIsIneligible_ShouldReturnFalseAsync(bool isActive, string accountType)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var context = CreateDematSqliteContext();
        var account = CreateDematAccount(901, "target@test.fr", "delivery@test.fr", "billing@test.fr");
        account.IsActive = isActive;
        account.AccountType = accountType;
        context.AccountEntity.Add(account);
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();
        var repository = new AccountRepository(context);

        var result = await repository.ResetDematEmailAsync(account.AccountId);

        var persistedAccount = await context.AccountEntity.IgnoreQueryFilters()
            .SingleAsync(entity => entity.AccountId == account.AccountId, cancellationToken);
        result.Should().BeFalse();
        persistedAccount.Email.Should().Be("target@test.fr");
    }

    /// <summary>
    /// Creates a relational in-memory Account context that supports set-based updates.
    /// </summary>
    /// <returns>The initialized Account context.</returns>
    private static AccountContext CreateDematSqliteContext()
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

    /// <summary>
    /// Creates an active client account for dematerialization email reset tests.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="email">The dematerialization email.</param>
    /// <param name="deliveryEmail">The delivery email.</param>
    /// <param name="billingEmail">The billing email.</param>
    /// <returns>The test account entity.</returns>
    private static AccountEntity CreateDematAccount(int accountId, string? email, string deliveryEmail, string billingEmail)
    {
        return new AccountEntity
        {
            AccountId = accountId,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = $"ACC{accountId}",
            LegalName = $"Account {accountId}",
            AccountType = AccountType.CLIENT.ToString(),
            Email = email!,
            DeliveryEmail = deliveryEmail,
            BillingEmail = billingEmail,
            IsActive = true,
            CreatedBy = "demat-reset-tests",
            CreationDate = DateTime.UtcNow,
        };
    }

    #endregion Reset demat email
}
