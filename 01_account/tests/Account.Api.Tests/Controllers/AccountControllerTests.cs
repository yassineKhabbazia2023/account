// <copyright file="AccountControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Pulse.Account.API;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
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
        var contactRepository = new Mock<IContactRepository>();
        var accountEventPublisher = new Mock<IAccountEventPublisher>();
        var featureFlagService = new Mock<IFeatureFlagService>();
        featureFlagService.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(r => r.UpdateLastActivityDateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        var accountService = new AccountService(accountRepository, contactRepository.Object, accountEventPublisher.Object, NullLogger<AccountService>.Instance, featureFlagService.Object, roleRepository.Object);
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
    public async Task Should_CreateAccount_ReturnsCreatedResultAsync()
    {
        // Arrange
        var currentUserId = 1;
        var request = new CreateAccountRequest
        {
            AccountNumber = "A12345",
            LegalName = "Account Test",
            Siret = "12345678900000",
            AccountType = AccountType.CLIENT
        };

        var expected = new AccountDetail
        {
            AccountId = 10,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = request.AccountNumber,
            Legal = new Legal { LegalName = request.LegalName },
            Phone = new List<Phone>()
        };

        var service = new Mock<IAccountService>();
        service.Setup(x => x.CreateAccountAsync(currentUserId, It.IsAny<CreateAccountRequest>())).ReturnsAsync(expected);
        var controller = new AccountController(service.Object);

        // Act
        var result = await controller.CreateAccountAsync(currentUserId, request);

        // Assert
        var createdResult = result.Result as CreatedResult;
        Assert.NotNull(createdResult);
        Assert.Equal(201, createdResult!.StatusCode);
        var response = createdResult.Value.As<CreateAccountResponse>();
        Assert.Equal("Compte cree avec succes", response.Message);
        Assert.Equal(expected.AccountId, response.AccountId);
    }

    [Fact]
    public async Task Should_CreateAccount_ReturnsBadRequest_WhenModelStateInvalidAsync()
    {
        // Arrange
        var currentUserId = 1;
        var request = new CreateAccountRequest
        {
            AccountNumber = string.Empty,
            LegalName = "Account Test",
            Siret = "12345678900000",
            AccountType = AccountType.CLIENT
        };

        var service = new Mock<IAccountService>(MockBehavior.Strict);
        var controller = new AccountController(service.Object);
        controller.ModelState.AddModelError(nameof(CreateAccountRequest.AccountNumber), "The AccountNumber field is required.");

        // Act
        var result = await controller.CreateAccountAsync(currentUserId, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Should_CreateAccount_ThrowsNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var currentUserId = 999;
        var request = new CreateAccountRequest
        {
            AccountNumber = "A12345",
            LegalName = "Account Test",
            Siret = "12345678900000",
            AccountType = AccountType.CLIENT
        };

        var service = new Mock<IAccountService>();
        service.Setup(x => x.CreateAccountAsync(currentUserId, It.IsAny<CreateAccountRequest>()))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, currentUserId)));
        var controller = new AccountController(service.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => controller.CreateAccountAsync(currentUserId, request));
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
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
        var account = await _accountController.GetAccountSummaryAsync(contactId, "Collaborator", accountMocked.AccountId);
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
        jsonPatch.Replace(a => a.Legal, new Legal { LegalName = "SAS TEST", Siren = "112233445" });
        jsonPatch.Replace(a => a.Phone, new List<Phone> { new Phone { PhoneNumber = "0625569262" } });

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

    /// <summary>
    /// Ensures the prospect-only contact endpoint returns the boolean value provided by the service.
    /// </summary>
    /// <param name="isProspectOnly">The service result.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsContactProspectOnlyAsync_Should_Return_Service_Result(bool isProspectOnly)
    {
        // Arrange
        var contactId = 6000;
        var accountService = new Mock<IAccountService>(MockBehavior.Strict);
        accountService.Setup(service => service.IsContactProspectOnlyAsync(contactId))
            .ReturnsAsync(isProspectOnly);
        var accountController = new AccountController(accountService.Object);

        // Act
        var result = await accountController.IsContactProspectOnlyAsync(contactId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(isProspectOnly, okResult.Value);
        accountService.Verify(service => service.IsContactProspectOnlyAsync(contactId), Times.Once);
    }

    /// <summary>
    /// Ensures the prospect-only contact endpoint propagates the existing contact-not-found exception.
    /// </summary>
    [Fact]
    public async Task IsContactProspectOnlyAsync_WhenContactDoesNotExist_Should_Throw_NotFoundException()
    {
        // Arrange
        var contactId = 6000;
        var accountService = new Mock<IAccountService>(MockBehavior.Strict);
        accountService.Setup(service => service.IsContactProspectOnlyAsync(contactId))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId)));
        var accountController = new AccountController(accountService.Object);

        // Act
        var result = async () => await accountController.IsContactProspectOnlyAsync(contactId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(result);
        Assert.Equal(Errors.NotFoundContactCode, exception.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, contactId), exception.Message);
    }

    /// <summary>
    /// Ensures the prospect-only contact endpoint keeps the requested downstream route shape.
    /// </summary>
    [Fact]
    public void IsContactProspectOnlyAsync_Should_Use_Expected_Route()
    {
        // Arrange
        var method = typeof(AccountController).GetMethod(nameof(AccountController.IsContactProspectOnlyAsync));

        // Act
        var attribute = Assert.Single(method!.GetCustomAttributes(typeof(HttpGetAttribute), false).OfType<HttpGetAttribute>());

        // Assert
        Assert.Equal("contacts/{contactId}/is-prospect-only", attribute.Template);
    }

    /// <summary>
    /// Ensures the contact widget endpoint only forwards the account identifier to the service.
    /// </summary>
    [Fact]
    public async Task GetAccountContactWidgetContactsAsync_Should_Returns_Account_Contacts()
    {
        // Arrange
        var accountId = 6000;
        var expected = _fixture.CreateMany<Contact>(2).ToList();

        var accountService = new Mock<IAccountService>(MockBehavior.Strict);
        accountService.Setup(service => service.GetAccountContactWidgetContactsAsync(accountId))
            .ReturnsAsync(expected);
        var accountController = new AccountController(accountService.Object);

        // Act
        var result = await accountController.GetAccountContactWidgetContactsAsync(accountId);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
        accountService.Verify(service => service.GetAccountContactWidgetContactsAsync(accountId), Times.Once);
        accountService.Verify(
            service => service.GetContactsAccountAsync(
                It.IsAny<int>(),
                It.IsAny<SearchContactsAccountCriteria>(),
                It.IsAny<Pagination>()),
            Times.Never);
    }

    /// <summary>
    /// Ensures the contact widget endpoint does not expose search criteria or pagination parameters.
    /// </summary>
    [Fact]
    public void GetAccountContactWidgetContactsAsync_Should_Only_Take_AccountId()
    {
        // Act
        var parameters = typeof(AccountController)
            .GetMethod(nameof(AccountController.GetAccountContactWidgetContactsAsync)) !
            .GetParameters();

        // Assert
        Assert.Single(parameters);
        Assert.Equal("accountId", parameters[0].Name);
        Assert.Equal(typeof(int), parameters[0].ParameterType);
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_UpdateAccount_InvalidAccountNumber_ReturnsBadRequest(string? invalidAccountNumber)
    {
        // Arrange
        var accountId = _context.AccountEntity.First().AccountId;
        var newHub = _fixture.Create<HubEntity>();
        _context.HubEntity.Add(newHub);
        _context.SaveChanges();

        var jsonPatch = new JsonPatchDocument<AccountDetail>();
        jsonPatch.Replace(a => a.Hub, new Hub { HubId = newHub.HubId, HubName = newHub.HubName });
        jsonPatch.Replace(a => a.AccountNumber, invalidAccountNumber);
        jsonPatch.Replace(a => a.Legal, new Legal { LegalName = "SAS TEST", Siren = "112233445" });
        jsonPatch.Replace(a => a.Phone, new List<Phone> { new Phone { PhoneNumber = "0625569262" } });

        // Act
        var result = await _accountController.UpdateAccountAsync(accountId, jsonPatch);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var serializableError = Assert.IsType<SerializableError>(badRequest.Value);

        // Recherche la clé sur AccountNumber (direct, ou "AccountNumber" pour model bind)
        Assert.Contains("AccountNumber", serializableError.Keys.Cast<string>());
        var errors = serializableError["AccountNumber"] as string[];
        Assert.NotNull(errors);
        Assert.Contains(errors!, e => e.Contains("Les champs obligatoires sont manquants ou invalides.", StringComparison.OrdinalIgnoreCase) ||
                                      e.Contains("Veuillez compléter les informations nécessaires.", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_UpdateAccount_InvalidLegalName_ReturnsBadRequest(string? invalidLegalName)
    {
        // Arrange
        var accountId = _context.AccountEntity.First().AccountId;
        var newHub = _fixture.Create<HubEntity>();
        _context.HubEntity.Add(newHub);
        _context.SaveChanges();

        var jsonPatch = new JsonPatchDocument<AccountDetail>();
        jsonPatch.Replace(a => a.Hub, new Hub { HubId = newHub.HubId, HubName = newHub.HubName });
        jsonPatch.Replace(a => a.AccountNumber, "A12345");
        jsonPatch.Replace(a => a.Legal, new Legal { LegalName = invalidLegalName, Siren = "112233445" });
        jsonPatch.Replace(a => a.Phone, new List<Phone> { new Phone { PhoneNumber = "0625569262" } });

        // Act
        var result = await _accountController.UpdateAccountAsync(accountId, jsonPatch);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var serializableError = Assert.IsType<SerializableError>(badRequest.Value);

        // Print les clés pour debugger
        foreach (var key in serializableError.Keys)
        {
            Console.WriteLine(key);
        }

        Assert.Contains("LegalName", serializableError.Keys.Cast<string>());
        var errors = serializableError["LegalName"] as string[];
        Assert.NotNull(errors);
        Assert.Contains(errors!, e => e.Contains("LegalName", StringComparison.OrdinalIgnoreCase)
                                      || e.Contains("vide", StringComparison.OrdinalIgnoreCase)
                                      || e.Contains("invalide", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_UpdateAccount_InvalidPhoneNumber_ReturnsBadRequest(string? invalidPhoneNumber)
    {
        // Arrange
        var accountId = _context.AccountEntity.First().AccountId;
        var newHub = _fixture.Create<HubEntity>();
        _context.HubEntity.Add(newHub);
        _context.SaveChanges();

        var jsonPatch = new JsonPatchDocument<AccountDetail>();
        jsonPatch.Replace(a => a.Hub, new Hub { HubId = newHub.HubId, HubName = newHub.HubName });
        jsonPatch.Replace(a => a.AccountNumber, "A12345");
        jsonPatch.Replace(a => a.Legal, new Legal { LegalName = "SAS TEST", Siren = "112233445" });
        jsonPatch.Replace(a => a.Phone, new List<Phone> { new Phone { PhoneNumber = invalidPhoneNumber } });

        // Act
        var result = await _accountController.UpdateAccountAsync(accountId, jsonPatch);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var serializableError = Assert.IsType<SerializableError>(badRequest.Value);

        Assert.Contains("PhoneNumber", serializableError.Keys.Cast<string>());
        var errors = serializableError["PhoneNumber"] as string[];
        Assert.NotNull(errors);
        Assert.Contains(errors!, e => e.Contains("obligatoires", StringComparison.OrdinalIgnoreCase) ||
                                      e.Contains("invalide", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UpdateAccountAsync_Should_Return_BadRequest_When_ModelState_IsInvalid()
    {
        // Arrange
        var accountId = _context.AccountEntity.First().AccountId;
        var jsonPatch = new JsonPatchDocument<AccountDetail>();

        // Simule un controller avec ModelState invalide (structure existante, utilisation de l'instance instanciée dans le ctor)
        _accountController.ModelState.AddModelError("TestField", "Erreur de validation");

        // Act
        var result = await _accountController.UpdateAccountAsync(accountId, jsonPatch);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsType<SerializableError>(badRequest.Value);
        Assert.True(errors.ContainsKey("TestField"));
    }

    [Fact]
    public async Task UpdateAccountAsync_Should_Throw_NotFoundException_When_Account_DoesNotExist()
    {
        // Arrange
        var nonExistentAccountId = int.MaxValue;
        var jsonPatch = new JsonPatchDocument<AccountDetail>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _accountController.UpdateAccountAsync(nonExistentAccountId, jsonPatch));

        Assert.Equal("L'entité avec l'identifiant 2147483647 est introuvable", exception.Message);
    }

    [Fact]
    public async Task UpdateAccountAsync_Should_Return_NotFound_When_GetAccountAsync_ReturnsNull()
    {
        // Arrange
        var mockService = new Mock<IAccountService>();
        mockService.Setup(s => s.GetAccountAsync(It.IsAny<int>())).ReturnsAsync((AccountDetail)null);

        var controller = new AccountController(mockService.Object);
        var jsonPatch = new JsonPatchDocument<AccountDetail>();

        // Act
        var result = await controller.UpdateAccountAsync(12345, jsonPatch); // n'importe quel Id

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SearchAccountsAsync_WithValidQuery_ShouldReturn200Ok()
    {
        // Arrange
        var mockService = new Mock<IAccountService>();
        var expectedResult = new Paging<AccountSearchResult>
        {
            Items = new List<AccountSearchResult>
            {
                new()
                {
                    AccountId = 1,
                    LegalName = "ACME Corporation",
                    AccountNumber = "ACC00123",
                    Siret = "12345678901234",
                    DirectorEmail = "director@acme.fr"
                }
            },
            CurrentPage = 1,
            TotalItems = 1,
            TotalPage = 1
        };

        mockService.Setup(s => s.SearchAccountsAsync(
            It.IsAny<string>(),
            It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        var controller = new AccountController(mockService.Object);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await controller.SearchAccountsAsync("ACME", pagination);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var value = okResult.Value as Paging<AccountSearchResult>;
        value.Should().NotBeNull();
        value!.Items.Should().HaveCount(1);
        value.Items.First().LegalName.Should().Be("ACME Corporation");
    }

    [Fact]
    public async Task SearchAccountsAsync_WithNoResults_ShouldReturn200OkWithEmptyList()
    {
        // Arrange
        var mockService = new Mock<IAccountService>();
        var expectedResult = new Paging<AccountSearchResult>
        {
            Items = new List<AccountSearchResult>(),
            CurrentPage = 1,
            TotalItems = 0,
            TotalPage = 0
        };

        mockService.Setup(s => s.SearchAccountsAsync(
            It.IsAny<string>(),
            It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        var controller = new AccountController(mockService.Object);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await controller.SearchAccountsAsync("NOTFOUND", pagination);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var value = okResult.Value as Paging<AccountSearchResult>;
        value.Should().NotBeNull();
        value!.Items.Should().BeEmpty();
        value.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task SearchAccountsAsync_WithInvalidPagination_ShouldValidateAndCallService()
    {
        // Arrange
        var mockService = new Mock<IAccountService>();
        var expectedResult = new Paging<AccountSearchResult>
        {
            Items = new List<AccountSearchResult>(),
            CurrentPage = 1,
            TotalItems = 0,
            TotalPage = 0
        };

        mockService.Setup(s => s.SearchAccountsAsync(
            It.IsAny<string>(),
            It.IsAny<Pagination>()))
            .ReturnsAsync(expectedResult);

        var controller = new AccountController(mockService.Object);
        var pagination = new Pagination { PageNumber = 0, PageSize = 0 }; // Invalid values

        // Act
        var result = await controller.SearchAccountsAsync("TEST", pagination);

        // Assert
        result.Should().NotBeNull();
        mockService.Verify(s => s.SearchAccountsAsync(
            It.IsAny<string>(),
            It.IsAny<Pagination>()), Times.Once);
    }
}
