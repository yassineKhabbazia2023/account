// <copyright file="AccountRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
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
                IsActive = true
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
                    .Create();
                var accountMock = _fixture.Build<AccountEntity>()
                                                .With(a => a.IsActive, true)
                                                .Without(a => a.Delegation)
                                                .Without(a => a.RoleEntity)
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
                context.SaveChanges();
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
                context.SaveChanges();
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
            var accounts = await accountRepository.GetAllAccountsAsync(null, pagination);

            // Assert
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
                                            .With(c => c.Type, "1")
                                            .Create();

            var deploymentMockActive = _fixture.Build<DeploymentEntity>()
                .With(a => a.Status, 1)
                .Create();

            var accountMock = _fixture.Build<AccountEntity>()
                                            .Without(a => a.Delegation)
                                            .Without(a => a.RoleEntity)
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
                                                .With(a => a.DeploymentEntity, deploymentMock)
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
            var accountFirst = accountsModel[0];
            var accountSummary = accountFirst?.MapToAccountDetail();
            context.AccountEntity.AddRange(accountsModel);
            await context.SaveChangesAsync();
            var accountRepository = new AccountRepository(context);

            // Act
            var accounts = await accountRepository.GetAccountSummaryAsync(accountFirst!.AccountId);

            // Assert
            Assert.Equal(accountSummary?.AccountNumber, accounts!.AccountNumber);
            Assert.Equal(accountSummary?.AccountId, accounts.AccountId);
            Assert.Equal(accountSummary?.Legal?.LegalName, accounts.LegalName);
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
            var accountsModel = _fixture.Build<AccountEntity>().With(a => a.IsActive, true).Create();
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

            context.SaveChanges();

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
            context.SaveChanges();
            context.ContactEntity.AddRange(contacts);
            context.SaveChanges();
            context.RoleEntity.AddRange(roles);
            context.SaveChanges();

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
                DeploymentStatus = (int)deploymentStatus,
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
            var result = await accountRepository.GetAllAccountsAsync(accountNumber, pagination);

            // Assert
            Assert.All(result.Items, item => Assert.Contains(accountNumber, item.AccountNumber));
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
            context.SaveChanges();
            context.RoleEntity.Add(role);
            context.SaveChanges();

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
        ContactType collaborator = ContactType.Collaborator;

        var account = _fixture.Build<AccountEntity>()
                .With(x => x.AccountId, accountId)
                .Without(x => x.RoleEntity)
                .Without(x => x.PhoneEntity)
                .Without(x => x.AddressEntity)
                .Without(x => x.DeploymentEntity)
                .Without(x => x.Office)
                .Without(x => x.Delegation)
                .Without(x => x.RoleLabelEntity)
                .With(x => x.IsActive, true)
                .Create();

        var contact1 = _fixture.Build<ContactEntity>()
            .With(x => x.FirstName, "Mostapha")
            .With(x => x.Type, ContactType.Collaborator.ToString())
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

        using (var context = new AccountContext(_dbContextOptions))
        {
            // Arrange
            context.AccountEntity.Add(account);
            context.ContactEntity.AddRange(new List<ContactEntity>() { contact1, contact2, contact3 });
            context.SaveChanges();
            context.RoleEntity.AddRange(new List<RoleEntity>() { role1, role2, role3 });
            context.SaveChanges();

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
            },
            new()
            {
                AccountId = 2,
                AccountGlobalUniqueId = Guid.NewGuid(),
                LegalName = "legal2",
                AccountNumber = "num2",
                CreatedBy = "moi",
                IsActive = true,
            },
            new()
            {
                AccountId = 3,
                AccountGlobalUniqueId = Guid.NewGuid(),
                LegalName = "legal3",
                AccountNumber = "num3",
                CreatedBy = "moi",
                IsActive = true,
            },
            new()
            {
                AccountId = 4,
                AccountGlobalUniqueId = Guid.NewGuid(),
                LegalName = "legal4",
                AccountNumber = "num4",
                CreatedBy = "moi",
                IsActive = true,
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
}
