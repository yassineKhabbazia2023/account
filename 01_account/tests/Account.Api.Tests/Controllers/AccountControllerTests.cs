// <copyright file="AccountControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pulse.Account.API;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Account.Api.Tests.Controllers;

public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;
    private readonly AccountController _accountController;
    private readonly AccountContext _context;

    public AccountControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = InitContext();
        var accountRepository = new AccountRepository(_context);
        var accountEventPublisher = new Mock<IAccountEventPublisher>();
        var accountService = new AccountService(accountRepository, accountEventPublisher.Object);
        _accountController = new AccountController(accountService);
    }

    ~AccountControllerTests()
    {
        _context.Dispose();
    }

    private AccountContext InitContext()
    {
        var context = new AccountContext(_dbContextOptions);
        var officeModel = _fixture.Build<OfficeEntity>().Create();
        var accountsModel = _fixture.Build<AccountEntity>().With(a => a.IsActive, true)
                                                                       .With(a => a.OfficeId, officeModel.OfficeId)
                                                                       .With(a => a.Office, officeModel)
                                                                       .Create();
        var accountsWithNullMissionTypeModel = _fixture.Build<AccountEntity>().With(a => a.IsActive, true)
                                                                                          .Create();
        accountsWithNullMissionTypeModel.OfficeId = null;
        accountsWithNullMissionTypeModel.Office = null;
        List<AccountEntity> accounts = [accountsModel, accountsWithNullMissionTypeModel];
        var contactsModel = _fixture.Build<ContactEntity>().With(a => a.IsActive, true).Create();

        context.AccountEntity.AddRange(accounts);
        context.ContactEntity.AddRange(contactsModel);
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task Should_GetAccountList_ReturnsOkResultAsync()
    {
        // Arrange
        var account = _context.AccountEntity.First();
        var contact = _context.ContactEntity.First();
        var searchAccountCriteria = new SearchAccountCriteria
        {
            ContactId = contact.ContactId
        };
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 4,
        };

        var expected = new Paging<AccountModel>
        {
            Items = new List<AccountModel>
            {
                account.MapToAccountSummary(contact.ContactId) !
            },
            CurrentPage = pagination.PageNumber,
            TotalItems = pagination.PageSize,
            TotalPage = 1,
        };

        var service = new Mock<IAccountService>();
        service.Setup(x => x.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>())).ReturnsAsync(expected);
        var controller = new AccountController(service.Object);

        // Act
        var accounts = await controller.GetAccountsAsync(searchAccountCriteria, pagination);
        var resultAccounts = accounts?.Result as OkObjectResult;

        // Assert
        Assert.Equal(account.AccountId, resultAccounts!.Value.As<Paging<AccountModel>>().Items!.First().AccountId);
    }

    [Fact]
    public async Task Should_GetAllAccounts_ReturnsOkResultAsync()
    {
        // Arrange
        var account = _context.AccountEntity.First();
        var contactId = 1;
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 4,
        };

        var expected = new Paging<AccountModel>
        {
            Items = new List<AccountModel>
            {
                account.MapToAccountSummary(contactId) !
            },
            CurrentPage = pagination.PageNumber,
            TotalItems = pagination.PageSize,
            TotalPage = 1,
        };

        var service = new Mock<IAccountService>();
        service.Setup(x => x.GetAllAccountsAsync(It.IsAny<string>(), It.IsAny<Pagination>(), It.IsAny<SearchAccountCriteria>())).ReturnsAsync(expected);
        var controller = new AccountController(service.Object);

        // Act
        var accounts = await controller.GetAllAccountsAsync(account.AccountNumber, pagination, null!);
        var resultAccounts = accounts?.Result as OkObjectResult;

        // Assert
        Assert.Equal(account.AccountId, resultAccounts!.Value.As<Paging<AccountModel>>().Items!.First().AccountId);
    }

    [Fact]
    public async Task Should_GetAccountSummary_ReturnsOkResultAsync()
    {
        // Arrange
        var accountMocked = await _context.AccountEntity.FirstAsync();
        var contactId = 1;
        var expected = accountMocked.MapToAccountSummary(contactId);

        // Act
        var account = await _accountController.GetAccountSummaryAsync(contactId, accountMocked.AccountId);
        var resultAccounts = account?.Result as OkObjectResult;
        var accountSummaryResult = resultAccounts!.Value.As<AccountModel>();

        // Assert
        Assert.NotNull(accountSummaryResult);
        Assert.Equivalent(expected, accountSummaryResult);
    }

    [Fact]
    public async Task Should_GetAccountDetail_ReturnsOkResultAsync()
    {
        // Arrange
        var accountMocked = await _context.AccountEntity.FirstAsync();
        var expected = accountMocked.MapToAccountDetail();

        // Act
        var account = await _accountController.GetAccountDetailAsync(accountMocked.AccountId);
        var resultAccounts = account?.Result as OkObjectResult;
        var accountDetailResult = resultAccounts!.Value.As<AccountDetail>();

        // Assert
        Assert.NotNull(accountDetailResult);
        Assert.Equivalent(expected, accountDetailResult);
    }

    [Fact]
    public async Task Should_UpdateAccount_ReturnsOkResultAsync()
    {
        // Arrange
        var accountId = _context.AccountEntity.First().AccountId;
        var newHub = _fixture.Create<HubEntity>();
        _context.HubEntity.Add(newHub);
        _context.SaveChanges();

        var jsonPatch = new JsonPatchDocument<AccountDetail>();
        jsonPatch.Replace(a => a.Hub, new Hub() { HubId = newHub.HubId, HubName = newHub.HubName });

        // Act
        var result = await _accountController.UpdateAccountAsync(accountId, jsonPatch) as OkResult;
        var accountDetail = await _accountController.GetAccountDetailAsync(accountId);
        var accountDetailResult = accountDetail.Result as OkObjectResult;

        // Assert
        Assert.Equal(200, result!.StatusCode);
        Assert.Equal(newHub.HubId, accountDetailResult!.Value.As<AccountDetail>().Hub!.HubId);
        Assert.Equal(newHub.HubName, accountDetailResult!.Value.As<AccountDetail>().Hub!.HubName);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithAccountPatchNull_ShouldThrowBadRequestException()
    {
        using (var context = InitContext())
        {
            // Arrange

            // Act
            var result = await Assert.ThrowsAsync<BadRequestException>(async () => await _accountController.UpdateAccountAsync(It.IsAny<int>(), null!));

            // Assert
            Assert.Equal(Errors.BadRequestAccountPatchCode, result.Code);
            Assert.Equal(Errors.BadRequestAccountPatchMessage, result.Message);
        }
    }

    [Fact]
    public async Task GetContactsAccountAsync_Should_Returns_Contacts_Account()
    {
        // Arrange
        var accountId = 6000;
        var expected = _fixture.Create<Paging<Contact>>();

        var accountService = new Mock<IAccountService>(MockBehavior.Strict);
        accountService.Setup(service => service.GetContactsAccountAsync(It.IsAny<int>(), It.IsAny<SearchContactsAccountCriteria>(), It.IsAny<Pagination>()))
            .ReturnsAsync(expected);
        var accountController = new AccountController(accountService.Object);
        var criteria = new SearchContactsAccountCriteria();

        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 4
        };

        // Act
        var result = await accountController.GetContactsAccountAsync(accountId, criteria, pagination);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_Should_Returns_Contacts_Account()
    {
        // Arrange
        var contactId = 6000;
        var expected = _fixture.Create<Paging<Contact>>();
        var request = new GetAssociatedContactsRequest
        {
            Search = string.Empty,
            ContactType = ContactType.Collaborator,
        };
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 999
        };

        var accountService = new Mock<IAccountService>(MockBehavior.Strict);
        accountService.Setup(service => service.GetAssociatedContactsAsync(contactId, request, pagination))
            .ReturnsAsync(expected);
        var accountController = new AccountController(accountService.Object);

        // Act
        var result = await accountController.GetAssociatedContactsAsync(contactId, request, pagination);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
    }

    [Fact]
    public async Task GetAssociatedContactsAsync_Should_Throw_NotFoundException()
    {
        // Arrange
        var contactId = 6000;
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 999
        };

        var accountService = new Mock<IAccountService>(MockBehavior.Strict);
        var request = new GetAssociatedContactsRequest
        {
            Search = string.Empty
        };

        accountService.Setup(service => service.GetAssociatedContactsAsync(contactId, request, pagination))
            .Throws(new NotFoundException(Errors.NotFoundRoleContactCode, Errors.NotFoundRoleContactMessage));
        var accountController = new AccountController(accountService.Object);

        // Act
        var result = async () => await accountController.GetAssociatedContactsAsync(contactId, request, pagination);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(result);
        Assert.Equal(Errors.NotFoundRoleContactCode, exception.Code);
        Assert.Equal(Errors.NotFoundRoleContactMessage, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ShouldReturn_ExpectedMissioTypeId_WithInvalidValue_GetAccountsAsyncInvoked(bool hasValue)
    {
        // Arrange
        var account = await _context.AccountEntity.FirstOrDefaultAsync(a => a.OfficeId.HasValue == hasValue);
        var contact = await _context.ContactEntity.FirstOrDefaultAsync();

        var searchAccountCriteria = new SearchAccountCriteria
        {
            ContactId = contact!.ContactId
        };
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 4,
        };

        var expected = new Paging<AccountModel>
        {
            Items = new List<AccountModel>
            {
                account!.MapToAccountSummary(contact!.ContactId)!
            },
            CurrentPage = pagination.PageNumber,
            TotalItems = pagination.PageSize,
            TotalPage = 1,
        };

        var service = new Mock<IAccountService>();
        service.Setup(x => x.GetAccountsAsync(It.IsAny<SearchAccountCriteria>(), It.IsAny<Pagination>())).ReturnsAsync(expected);
        var controller = new AccountController(service.Object);

        // Act
        var accounts = await controller.GetAccountsAsync(searchAccountCriteria, pagination);
        var resultAccounts = accounts?.Result as OkObjectResult;

        // Assert
        Assert.NotNull(account);
        Assert.Equal(account.OfficeId, resultAccounts!.Value.As<Paging<AccountModel>>().Items!.First().OfficeId);
    }
}
