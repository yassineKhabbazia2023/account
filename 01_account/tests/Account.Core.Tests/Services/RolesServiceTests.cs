// <copyright file="RolesServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;
using PulseInvalidOperationException = Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException;

namespace Pulse.Account.Core.Tests.Services;

public class RolesServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Mock<IContactRepository> _contactRepository;
    private readonly Mock<IRoleEventPublisher>? _rolePublisher;
    private readonly Mock<IHistoryEventPublisher>? _historyPublisher;
    private readonly Mock<IRoleLabelService> _roleLabelService;
    private readonly Mock<ILogger<RolesService>>? _logger;
    private readonly Mock<IFeatureFlagService> _featureFlagService;
    private readonly Fixture _fixture;

    public RolesServiceTests()
    {
        _roleRepository = new Mock<IRoleRepository>();
        _contactRepository = new Mock<IContactRepository>();
        _rolePublisher = new Mock<IRoleEventPublisher>();
        _historyPublisher = new Mock<IHistoryEventPublisher>();
        _roleLabelService = new Mock<IRoleLabelService>();
        _logger = new Mock<ILogger<RolesService>>();
        _featureFlagService = new Mock<IFeatureFlagService>();
        _featureFlagService.Setup(f => f.IsEnabledAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetContactRolesAsync_Should_ReturnsOkResultAsync()
    {
        // Arrange
        var accountList = _fixture.Create<Paging<AccountModel>>();
        _roleRepository.Setup(repository => repository.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<Pagination>())).ReturnsAsync(accountList);
        var rolesService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var accounts = await rolesService.GetContactRolesAsync(contactId: 123, null!);

        // Assert
        Assert.Equal(accountList, accounts);
        _roleRepository.Verify(x => x.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<Pagination>()));
    }

    [Fact]
    public async Task GetSignatoryAsync_Should_Returns_Account_Signatory()
    {
        // Arrange
        var accountId = 116;
        var expected = new List<Contact> { _fixture.Create<Contact>() };

        _roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(accountId))
        .ReturnsAsync(expected);

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.GetSignatoryAsync(accountId);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateRoleAsync_Should_ReturnsCreatedResult()
    {
        // Arrange
        var newRole = new Role()
        {
            AccountId = 6,
            ContactId = 6,
            IsFavorite = false,
            IsSignatory = false
        };
        var createRoleRequest = new CreateRoleRequest()
        {
            AccountId = 6,
            ContactId = 6,
            IsFavorite = false,
            IsSignatory = false,
            IncludePennylaneAccess = true
        };
        var contactId = 123;

        _roleRepository.Setup(repo => repo.CreateRoleAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(newRole)
            .Verifiable();

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        _rolePublisher!.Setup(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Callback<CreateRoleRequest, string, bool>((role, _, _) =>
            {
                role.Should().NotBeNull();
                role.ContactId.Should().Be(newRole.ContactId);
                role.AccountId.Should().Be(newRole.AccountId);
                role.IsSignatory.Should().Be(false);
            })
         .Returns(Task.CompletedTask)
         .Verifiable();

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        await roleService.CreateRoleAsync(createRoleRequest, contactId);

        // Assert
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        _historyPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldPublishWithIncludeProspectsTrue()
    {
        // Arrange
        var newRole = new Role { AccountId = 6, ContactId = 6 };
        var createRoleRequest = new CreateRoleRequest { AccountId = 6, ContactId = 6 };

        _roleRepository.Setup(repo => repo.CreateRoleAsync(It.IsAny<CreateRoleRequest>())).ReturnsAsync(newRole);
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());
        _rolePublisher!.Setup(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        await roleService.CreateRoleAsync(createRoleRequest, 123);

        // Assert
        _rolePublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), true), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenProspectAccountWithoutExclusiveLabel_ShouldSucceed()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        roleRepository.Setup(repo => repo.IsProspectAccountAsync(1)).ReturnsAsync(true);
        _roleLabelService.Setup(s => s.HasExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), true), Times.Once);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldThrow_BadRequestException()
    {
        // Arrange
        _roleRepository.Setup(repo => repo.CreateRoleAsync(null!))
            .ThrowsAsync(new BadRequestException(Errors.NotFoundAccountMessage, Errors.NotFoundAccountMessage));
        var contactId = 123;

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task Roles() => roleService.CreateRoleAsync(null!, contactId);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(Roles);
        _rolePublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _historyPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateRolesAsync_Should_ReturnAllSucceeded_WhenAllContactsCreate()
    {
        // Arrange
        var accountId = 10;
        var currentUserId = 25;
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 1, IsSignatory = true },
                new() { ContactId = 2, IsSignatory = false },
                new() { ContactId = 3, IsSignatory = false }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Contact
            {
                ContactId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.fr",
                Type = "Customer",
            });

        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync((CreateRoleRequest r) => new Role { AccountId = r.AccountId, ContactId = r.ContactId!.Value, IsSignatory = r.IsSignatory });

        _rolePublisher!.Setup(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(accountId, request, currentUserId);

        // Assert
        result.Succeeded.Should().HaveCount(3);
        result.Succeeded.Select(s => s.ContactId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(
            r => r.CreateRoleWithoutAccountValidationAsync(It.Is<CreateRoleRequest>(x => x.AccountId == accountId)),
            Times.Exactly(3));
        _rolePublisher.Verify(
            publisher => publisher.PublishRoleCreatedEventAsync(
                It.Is<CreateRoleRequest>(request => request.AccountId == accountId && request.DelegatorId == currentUserId),
                It.IsAny<string>(),
                It.Is<bool>(includeProspects => includeProspects)),
            Times.Exactly(3));
    }

    [Fact]
    public async Task CreateRolesAsync_Should_ReturnFailed_WhenRoleAlreadyExists()
    {
        // Arrange
        var accountId = 10;
        var currentUserId = 25;
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 1, IsSignatory = true },
                new() { ContactId = 2, IsSignatory = false }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Contact
            {
                ContactId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.fr",
                Type = "Customer",
            });

        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.Is<CreateRoleRequest>(r => r.ContactId == 1)))
            .ReturnsAsync(new Role { AccountId = accountId, ContactId = 1 });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.Is<CreateRoleRequest>(r => r.ContactId == 2)))
            .ThrowsAsync(new ConflictException(Errors.BadRequestExistingRoleCode, string.Format(Errors.BadRequestExistingRoleMessage, 2, accountId)));

        _rolePublisher!.Setup(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(accountId, request, currentUserId);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 1);
        result.Failed.Should().ContainSingle(f => f.ContactId == 2 && f.ErrorCode == Errors.BadRequestExistingRoleCode);
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Exactly(2));
        _rolePublisher.Verify(
            publisher => publisher.PublishRoleCreatedEventAsync(
                It.Is<CreateRoleRequest>(request => request.ContactId == 1 && request.AccountId == accountId && request.DelegatorId == currentUserId),
                It.IsAny<string>(),
                It.Is<bool>(includeProspects => includeProspects)),
            Times.Once);
    }

    [Fact]
    public async Task CreateRolesAsync_Should_ReturnFailed_WhenContactNotFound()
    {
        // Arrange
        var accountId = 10;
        var currentUserId = 25;
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem> { new() { ContactId = 99, IsSignatory = false } }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(99))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, 99)));

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(accountId, request, currentUserId);

        // Assert
        result.Succeeded.Should().BeEmpty();
        result.Failed.Should().ContainSingle(f => f.ContactId == 99 && f.ErrorCode == Errors.NotFoundContactCode);
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreateRolesAsync_Should_ReturnEmpty_WhenContactsListIsEmpty()
    {
        // Arrange
        var request = new CreateRolesBulkRequest { Contacts = new List<CreateRolesBulkItem>() };
        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().BeEmpty();
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreateRolesAsync_Should_ReturnEmpty_WhenRequestIsNull()
    {
        // Arrange
        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, null!, 25);

        // Assert
        result.Succeeded.Should().BeEmpty();
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreateRolesAsync_Should_SetIsCustomerRelationToTrue_WhenContactIsCollaborator()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 42, IsSignatory = false }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(42))
            .ReturnsAsync(new Contact
            {
                ContactId = 42,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@test.fr",
                Type = "Collaborator",
            });

        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(
                It.Is<CreateRoleRequest>(r =>
                    r.AccountId == 10 &&
                    r.ContactId == 42 &&
                    r.IsCustomerRelation == true)))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 42 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 42);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(repo => repo.CreateRoleWithoutAccountValidationAsync(
            It.Is<CreateRoleRequest>(r => r.ContactId == 42 && r.IsCustomerRelation == true)), Times.Once);
    }

    [Fact]
    public async Task CreateRolesBulkAsync_WithRoleCodeAm_ShouldCallAssignRoleLabel()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 7, RoleCode = "AM" }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(7))
            .ReturnsAsync(new Contact { ContactId = 7, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Customer" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 7 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 7);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("AM", 10, 7), Times.Once);
    }

    [Fact]
    public async Task CreateRolesBulkAsync_WithRoleCodeClp_ShouldCallAssignRoleLabel()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 8, RoleCode = "CLP" }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(8))
            .ReturnsAsync(new Contact { ContactId = 8, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Customer" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 8 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 8);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("CLP", 10, 8), Times.Once);
    }

    /// <summary>
    /// Verifies that one collaborator can receive both prospect role labels from the same bulk request.
    /// </summary>
    [Fact]
    public async Task CreateRolesBulkAsync_WithSameContactAndBothRoleCodes_ShouldCreateRoleOnceAndAssignBothLabels()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 7, RoleCode = "AM" },
                new() { ContactId = 7, RoleCode = "CLP" }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(7))
            .ReturnsAsync(new Contact { ContactId = 7, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Collaborator" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 7 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().HaveCount(2);
        result.Succeeded.Should().OnlyContain(s => s.ContactId == 7);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Once);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("AM", 10, 7), Times.Once);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("CLP", 10, 7), Times.Once);
    }

    /// <summary>
    /// Verifies that repeated labels for the same collaborator reuse the same base role.
    /// </summary>
    [Fact]
    public async Task CreateRolesBulkAsync_WithSameContactAndSameRoleCode_ShouldReuseBaseRole()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 7, RoleCode = "AM" },
                new() { ContactId = 7, RoleCode = "AM" }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(7))
            .ReturnsAsync(new Contact { ContactId = 7, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Collaborator" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 7 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().HaveCount(2);
        result.Succeeded.Should().OnlyContain(s => s.ContactId == 7);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Once);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("AM", 10, 7), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that retrying a bulk request still assigns labels when the base role already exists.
    /// </summary>
    [Fact]
    public async Task CreateRolesBulkAsync_WhenSameContactRoleAlreadyExists_ShouldAssignBothLabels()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 7, RoleCode = "AM" },
                new() { ContactId = 7, RoleCode = "CLP" }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(7))
            .ReturnsAsync(new Contact { ContactId = 7, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Collaborator" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ThrowsAsync(new ConflictException(Errors.BadRequestExistingRoleCode, string.Format(Errors.BadRequestExistingRoleMessage, 7, 10)));

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().HaveCount(2);
        result.Succeeded.Should().OnlyContain(s => s.ContactId == 7);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Once);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("AM", 10, 7), Times.Once);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("CLP", 10, 7), Times.Once);
    }

    [Fact]
    public async Task CreateRolesBulkAsync_WithNullRoleCode_ShouldStillCallServiceWithNull()
    {
        // Arrange : RoleCode null → on délègue quand même à AssignRoleLabelFromCodeAsync qui gère le no-op.
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 9 }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(9))
            .ReturnsAsync(new Contact { ContactId = 9, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Customer" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 9 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 9);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync(null, 10, 9), Times.Once);
    }

    [Fact]
    public async Task CreateRolesBulkAsync_WhenLabelAssignmentThrows_ItemShouldRemainSucceeded()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 11, RoleCode = "AM" }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(11))
            .ReturnsAsync(new Contact { ContactId = 11, FirstName = "F", LastName = "L", Email = "e@e.fr", Type = "Customer" });
        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 11 });
        _roleLabelService.Setup(r => r.AssignRoleLabelFromCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new ConflictException(Errors.RoleLabelAlreadyExistsCode, Errors.RoleLabelAlreadyExistsMessage));

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 11);
        result.Failed.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateRolesAsync_Should_KeepIsCustomerRelationUnchanged_WhenContactIsNotCollaborator()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 24, IsSignatory = true }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(24))
            .ReturnsAsync(new Contact
            {
                ContactId = 24,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@test.fr",
                Type = "Customer",
            });

        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(
                It.Is<CreateRoleRequest>(r =>
                    r.AccountId == 10 &&
                    r.ContactId == 24 &&
                    r.IsCustomerRelation == null)))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 24, IsSignatory = true });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 24);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(repo => repo.CreateRoleWithoutAccountValidationAsync(
            It.Is<CreateRoleRequest>(r => r.ContactId == 24 && r.IsCustomerRelation == null)), Times.Once);
    }

    [Fact]
    public async Task CreateRolesBulkAsync_Should_PropagateContactFlagPortailFactures_WhenSetInRequest()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 55, IsSignatory = true, ContactFlagPortailFactures = true }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(55))
            .ReturnsAsync(new Contact { ContactId = 55, FirstName = "A", LastName = "B", Email = "a@b.fr", Type = "Customer" });

        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(
                It.Is<CreateRoleRequest>(r => r.ContactId == 55 && r.ContactFlagPortailFactures == true)))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 55 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 55);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(repo => repo.CreateRoleWithoutAccountValidationAsync(
            It.Is<CreateRoleRequest>(r => r.ContactId == 55 && r.ContactFlagPortailFactures == true)), Times.Once);
    }

    [Fact]
    public async Task CreateRolesBulkAsync_Should_LeaveContactFlagPortailFacturesNull_WhenNotSetInRequest()
    {
        // Arrange
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 56, IsSignatory = false }
            }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(56))
            .ReturnsAsync(new Contact { ContactId = 56, FirstName = "C", LastName = "D", Email = "c@d.fr", Type = "Customer" });

        _roleRepository.Setup(repo => repo.CreateRoleWithoutAccountValidationAsync(
                It.Is<CreateRoleRequest>(r => r.ContactId == 56 && r.ContactFlagPortailFactures == null)))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = 56 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 56);
        _roleRepository.Verify(repo => repo.CreateRoleWithoutAccountValidationAsync(
            It.Is<CreateRoleRequest>(r => r.ContactId == 56 && r.ContactFlagPortailFactures == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleSignatoryAsync_Should_ReturnsOkResultAsync()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        var newRole = new Role()
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = false,
            IsSignatory = false
        };

        _roleRepository.Setup(repo => repo.UpdateRoleSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(newRole)
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
          .Callback<int, int>((accId, contId) =>
          {
              contId.Should().Be(contactId);
              accId.Should().Be(accountId);
          })
          .Returns(Task.CompletedTask)
          .Verifiable();

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        await roleService.UpdateRoleSignatoryAsync(1, accountId, contactId, true);

        // Assert
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _historyPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(1, contactId, accountId, "ADDSIGNMANU"), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleSignatoryAsync_WhenRemovingSignatory_ShouldPublishDelsignmanuHistory()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        var newRole = new Role()
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = false,
            IsSignatory = true
        };

        _roleRepository.Setup(repo => repo.UpdateRoleSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(newRole)
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
          .Callback<int, int>((accId, contId) =>
          {
              contId.Should().Be(contactId);
              accId.Should().Be(accountId);
          })
          .Returns(Task.CompletedTask)
          .Verifiable();

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        await roleService.UpdateRoleSignatoryAsync(1, accountId, contactId, false);

        // Assert
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _historyPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(1, contactId, accountId, "DELSIGNMANU"), Times.Once);
    }

    private static Mock<IRoleRepository> DeleteRole_MockRepo()
    {
        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        var roleNormal = new Role
        {
            AccountId = 1,
            ContactId = 1,
            IsFavorite = false,
            IsSignatory = false,
        };
        var roleSignatory = new Role
        {
            AccountId = 1,
            ContactId = 3,
            IsFavorite = false,
            IsSignatory = true,
        };
        roleRepository.Setup(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        roleRepository.Setup(repo => repo.GetContactRoleAsync(1, 1))
            .ReturnsAsync(roleNormal);
        roleRepository.Setup(repo => repo.GetContactRoleAsync(1, 3))
            .ReturnsAsync(roleSignatory);
        roleRepository.Setup(repo => repo.GetContactRoleAsync(1, 2))
            .ThrowsAsync(new NotFoundException(It.IsAny<string>(), It.IsAny<string>()));
        roleRepository.Setup(repo => repo.IsProspectAccountAsync(It.IsAny<int>())).ReturnsAsync(false);

        return roleRepository;
    }

    [Fact]
    public void DeleteRoleAsync_ShouldDeleteRole()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public void DeleteRoleAsync_ShouldDeleteRole_Cas2Signatory()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>())).ReturnsAsync(_fixture.CreateMany<Contact>(2));
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrow_NotFoundException()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService.DeleteRoleAsync(currentUserId, 1, 2);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(DeleteRole);
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrow_BadRequestException()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contact> { _fixture.Create<Contact>() });
        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService.DeleteRoleAsync(currentUserId, 1, 3);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void DeleteRoleAsync_ForProspectAccount_ShouldPublishWithIncludeProspectsTrue()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        roleRepository.Setup(repo => repo.IsProspectAccountAsync(1)).ReturnsAsync(true);
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), true), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrow_BadRequestException_WhenProspectCollabHasExclusiveLabel()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        roleRepository.Setup(repo => repo.IsProspectAccountAsync(1)).ReturnsAsync(true);
        _roleLabelService.Setup(s => s.HasExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void DeleteRoleAsync_ShouldNotThrow_WhenNonProspectCollabHasExclusiveLabel()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        roleRepository.Setup(repo => repo.IsProspectAccountAsync(1)).ReturnsAsync(false);
        _roleLabelService.Setup(s => s.HasExclusiveLabelAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ReturnsOkResultAsync()
    {
        // Arrange
        _roleRepository.Setup(repository => repository.CheckRoleExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
        var rolesService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var contactHasRoleOnAccount = await rolesService.CheckRoleExistsAsync(2, 1, 1, "test@test.fr");

        // Assert
        _roleRepository.Verify(x => x.CheckRoleExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()));
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task IsContactHasRoleInAccount_ReturnsTrueAsync()
    {
        // Arrange
        _roleRepository.Setup(repository => repository.IsContactHasRoleOnAccount(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
        var rolesService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var contactHasRoleOnAccount = await rolesService.IsContactHasRoleOnAccount(1, 1, "accountNumber");

        // Assert
        _roleRepository.Verify(x => x.IsContactHasRoleOnAccount(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_Should_Return_Updated_Role_For_Single_Account()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        bool isCustomerRelation = true;

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = new List<int> { accountId }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, request.AccountIds))
            .ReturnsAsync(new List<Role> { new() { AccountId = accountId, ContactId = contactId } });

        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, request.AccountIds))
            .ReturnsAsync(new HashSet<int>());

        _roleRepository.Setup(repo => repo.UpdateRoleCustomerRelationAsync(contactId, isCustomerRelation, It.Is<Dictionary<int, int>>(d => d.ContainsKey(accountId) && d[accountId] == (int)ActionLevelType.DirectClientRelation)))
            .ReturnsAsync(new UpdateRoleCustomerRelationResponse { Succeeded = new List<UpdateRoleCustomerRelationItemResponse> { new() { AccountId = accountId } } })
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().ContainSingle(x => x.AccountId == accountId);
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(accountId, contactId), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_Should_Return_Updated_Roles_For_Multiple_Accounts()
    {
        // Arrange
        var accountIds = new List<int> { 10, 11, 12 };
        int contactId = 25;
        bool isCustomerRelation = true;

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = accountIds
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, accountIds))
            .ReturnsAsync(accountIds.Select(id => new Role { AccountId = id, ContactId = contactId }).ToList());

        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, accountIds))
            .ReturnsAsync(new HashSet<int>());

        _roleRepository.Setup(repo => repo.UpdateRoleCustomerRelationAsync(contactId, isCustomerRelation, It.Is<Dictionary<int, int>>(d => accountIds.All(id => d.ContainsKey(id)))))
            .ReturnsAsync(new UpdateRoleCustomerRelationResponse { Succeeded = accountIds.Select(id => new UpdateRoleCustomerRelationItemResponse { AccountId = id }).ToList() })
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().HaveCount(3);
        result.Failed.Should().BeEmpty();
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), contactId), Times.Exactly(3));
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_Should_Publish_Event_Only_For_Succeeded_Updates()
    {
        // Arrange
        var accountIds = new List<int> { 10, 11 };
        int contactId = 25;
        bool isCustomerRelation = false;

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = accountIds
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, accountIds))
            .ReturnsAsync(new List<Role> { new() { AccountId = 10, ContactId = contactId } });

        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, accountIds))
            .ReturnsAsync(new HashSet<int>());

        _roleRepository.Setup(repo => repo.UpdateRoleCustomerRelationAsync(contactId, isCustomerRelation, It.Is<Dictionary<int, int>>(d => d.ContainsKey(10) && d[10] == (int)ActionLevelType.Observator)))
            .ReturnsAsync(new UpdateRoleCustomerRelationResponse
            {
                Succeeded = new List<UpdateRoleCustomerRelationItemResponse> { new() { AccountId = 10 } }
            })
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().ContainSingle(x => x.AccountId == 10);
        result.Failed.Should().ContainSingle(x => x.AccountId == 11);
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(10, contactId), Times.Once);
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(11, contactId), Times.Never);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_Should_Throw_InvalidOperationException_When_Contact_Is_Not_Collaborator()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        bool isCustomerRelation = true;

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = new List<int> { accountId }
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Customer.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        Func<Task> act = async () => await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Customer.ToString(), request);

        // Assert
        await act.Should().ThrowAsync<PulseInvalidOperationException>()
            .WithMessage(Errors.NoClientLabelMessage);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_WithRoleLabels_Should_SetActionLevelToContributorForLabeledAccounts()
    {
        // Arrange
        const int labeledAccountId = 10;
        const int unlabeledAccountId = 11;
        int contactId = 25;
        bool isCustomerRelation = false;
        var accountIds = new List<int> { labeledAccountId, unlabeledAccountId };

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = accountIds
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, accountIds))
            .ReturnsAsync(new List<Role>
            {
                new() { AccountId = labeledAccountId, ContactId = contactId },
                new() { AccountId = unlabeledAccountId, ContactId = contactId }
            });

        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, accountIds))
            .ReturnsAsync(new HashSet<int> { labeledAccountId });

        _roleRepository.Setup(repo => repo.UpdateRoleCustomerRelationAsync(contactId, isCustomerRelation, It.Is<Dictionary<int, int>>(d =>
                d[labeledAccountId] == (int)ActionLevelType.Contributor &&
                d[unlabeledAccountId] == (int)ActionLevelType.Observator)))
            .ReturnsAsync(new UpdateRoleCustomerRelationResponse
            {
                Succeeded = accountIds.Select(id => new UpdateRoleCustomerRelationItemResponse { AccountId = id }).ToList()
            })
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().HaveCount(2);
        result.Failed.Should().BeEmpty();
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(labeledAccountId, contactId), Times.Once);
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(unlabeledAccountId, contactId), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_WithDuplicateAccountIds_Should_ProcessOnlyOnce()
    {
        // Arrange
        const int accountId = 10;
        const int contactId = 25;
        const bool isCustomerRelation = true;
        var accountIds = new List<int> { accountId, accountId };

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = accountIds
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, It.Is<List<int>>(list => list.Count == 1 && list.Contains(accountId))))
            .ReturnsAsync(new List<Role> { new() { AccountId = accountId, ContactId = contactId } });

        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, It.Is<List<int>>(list => list.Count == 1 && list.Contains(accountId))))
            .ReturnsAsync(new HashSet<int>());

        _roleRepository.Setup(repo => repo.UpdateRoleCustomerRelationAsync(contactId, isCustomerRelation, It.Is<Dictionary<int, int>>(d => d.Count == 1 && d.ContainsKey(accountId))))
            .ReturnsAsync(new UpdateRoleCustomerRelationResponse { Succeeded = new List<UpdateRoleCustomerRelationItemResponse> { new() { AccountId = accountId } } })
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().ContainSingle(x => x.AccountId == accountId);
        result.Failed.Should().BeEmpty();
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(accountId, contactId), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_WhenRepositoryReturnsFailed_Should_MergeFailedResults()
    {
        // Arrange
        const int contactId = 25;
        const bool isCustomerRelation = true;
        var accountIds = new List<int> { 10, 11 };

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = accountIds
        };

        _contactRepository.Setup(repo => repo.GetContactByIdAsync(contactId))
            .ReturnsAsync(new Contact { ContactId = contactId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, accountIds))
            .ReturnsAsync(accountIds.Select(id => new Role { AccountId = id, ContactId = contactId }).ToList());

        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, accountIds))
            .ReturnsAsync(new HashSet<int>());

        _roleRepository.Setup(repo => repo.UpdateRoleCustomerRelationAsync(contactId, isCustomerRelation, It.Is<Dictionary<int, int>>(d => accountIds.All(id => d.ContainsKey(id)))))
            .ReturnsAsync(new UpdateRoleCustomerRelationResponse
            {
                Succeeded = new List<UpdateRoleCustomerRelationItemResponse> { new() { AccountId = 10 } },
                Failed = new List<UpdateRoleCustomerRelationItemResponse>
                {
                    new()
                    {
                        AccountId = 11,
                        ErrorCode = Errors.NotFoundRoleCode,
                        ErrorMessage = string.Format(Errors.NotFoundRoleMessage, contactId, 11)
                    }
                }
            })
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().ContainSingle(x => x.AccountId == 10);
        result.Failed.Should().ContainSingle(x => x.AccountId == 11);
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(10, contactId), Times.Once);
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(11, contactId), Times.Never);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_WhenNoMatchingRoles_ShouldReturnEmptySucceeded()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        bool isCustomerRelation = true;

        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = new List<int> { accountId }
        };

        _roleRepository.Setup(repo => repo.GetRolesByContactAndAccountIdsAsync(contactId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Role>());
        _roleRepository.Setup(repo => repo.GetAccountIdsWithRoleLabelAsync(contactId, It.IsAny<List<int>>()))
            .ReturnsAsync(new HashSet<int>());

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkUpdateRoleCustomerRelationAsync(contactId, ContactType.Collaborator.ToString(), request);

        // Assert
        result.Succeeded.Should().BeEmpty();
        result.Failed.Should().ContainSingle(x => x.AccountId == accountId);
    }

    [Fact]
    public async Task BulkDeleteRolesAsync_Should_ReturnAllSucceeded_WhenAllRemovable()
    {
        // Arrange
        var currentUserId = 25;
        var request = new BulkRoleDeleteRequest { AccountIds = new List<int> { 10, 20, 30 } };

        _roleRepository.Setup(r => r.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int accountId, int contactId) => new Role { AccountId = accountId, ContactId = contactId, IsSignatory = false });
        _roleRepository.Setup(r => r.IsProspectAccountAsync(It.IsAny<int>())).ReturnsAsync(false);
        _roleRepository.Setup(r => r.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Contact { ContactId = currentUserId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkDeleteRolesAsync(currentUserId, request);

        // Assert
        result.Succeeded.Select(s => s.AccountId).Should().BeEquivalentTo(new[] { 10, 20, 30 });
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.DeleteRoleAsync(It.IsAny<int>(), currentUserId), Times.Exactly(3));
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), currentUserId, It.IsAny<bool>()), Times.Exactly(3));
    }

    [Fact]
    public async Task BulkDeleteRolesAsync_Should_ReturnFailed_WhenRoleNotFound()
    {
        // Arrange
        var currentUserId = 25;
        var request = new BulkRoleDeleteRequest { AccountIds = new List<int> { 10 } };

        _roleRepository.Setup(r => r.GetContactRoleAsync(10, currentUserId)).ReturnsAsync((Role?)null);

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkDeleteRolesAsync(currentUserId, request);

        // Assert
        result.Succeeded.Should().BeEmpty();
        result.Failed.Should().ContainSingle(f => f.AccountId == 10 && f.ErrorCode == Errors.NotFoundRoleCode);
        _roleRepository.Verify(r => r.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task BulkDeleteRolesAsync_Should_ReturnPartialResult_WhenSomeFail()
    {
        // Arrange
        var currentUserId = 25;
        var request = new BulkRoleDeleteRequest { AccountIds = new List<int> { 10, 20 } };

        _roleRepository.Setup(r => r.GetContactRoleAsync(10, currentUserId))
            .ReturnsAsync(new Role { AccountId = 10, ContactId = currentUserId, IsSignatory = false });
        _roleRepository.Setup(r => r.GetContactRoleAsync(20, currentUserId)).ReturnsAsync((Role?)null);
        _roleRepository.Setup(r => r.IsProspectAccountAsync(It.IsAny<int>())).ReturnsAsync(false);
        _roleRepository.Setup(r => r.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Contact { ContactId = currentUserId, Type = ContactType.Collaborator.ToString(), FirstName = "Test", LastName = "User", Email = "test.user@test.fr" });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkDeleteRolesAsync(currentUserId, request);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.AccountId == 10);
        result.Failed.Should().ContainSingle(f => f.AccountId == 20 && f.ErrorCode == Errors.NotFoundRoleCode);
        _roleRepository.Verify(r => r.DeleteRoleAsync(10, currentUserId), Times.Once);
        _roleRepository.Verify(r => r.DeleteRoleAsync(20, currentUserId), Times.Never);
    }

    [Fact]
    public async Task BulkDeleteRolesAsync_Should_ReturnEmptyResult_WhenAccountIdsEmpty()
    {
        // Arrange
        var currentUserId = 25;
        var request = new BulkRoleDeleteRequest { AccountIds = new List<int>() };

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.BulkDeleteRolesAsync(currentUserId, request);

        // Assert
        result.Succeeded.Should().BeEmpty();
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(r => r.GetContactRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _roleRepository.Verify(r => r.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CheckLastCollaboratorAsync_Should_ReturnFlagsAndAccountIds_WhenLastOnSome()
    {
        // Arrange
        var contactId = 25;
        var accountIds = new List<int> { 10, 20, 30 };

        _roleRepository.Setup(r => r.GetAccountsWhereContactIsLastCollaboratorAsync(contactId, It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(new List<int> { 10, 30 });

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CheckLastCollaboratorAsync(contactId, accountIds);

        // Assert
        result.IsLastCollaboratorOnAny.Should().BeTrue();
        result.AccountIdsWhereLastCollaborator.Should().BeEquivalentTo(new[] { 10, 30 });
        _roleRepository.Verify(r => r.GetAccountsWhereContactIsLastCollaboratorAsync(contactId, It.IsAny<IReadOnlyCollection<int>>()), Times.Once);
    }

    [Fact]
    public async Task CheckLastCollaboratorAsync_Should_ReturnFalse_WhenLastOnNone()
    {
        // Arrange
        var contactId = 25;
        var accountIds = new List<int> { 10, 20 };

        _roleRepository.Setup(r => r.GetAccountsWhereContactIsLastCollaboratorAsync(contactId, It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(new List<int>());

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object, _featureFlagService.Object);

        // Act
        var result = await roleService.CheckLastCollaboratorAsync(contactId, accountIds);

        // Assert
        result.IsLastCollaboratorOnAny.Should().BeFalse();
        result.AccountIdsWhereLastCollaborator.Should().BeEmpty();
    }
}
