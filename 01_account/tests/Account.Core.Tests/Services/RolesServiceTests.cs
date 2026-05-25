// <copyright file="RolesServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Core.Tests.Services;

public class RolesServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Mock<IContactRepository> _contactRepository;
    private readonly Mock<IRoleEventPublisher>? _rolePublisher;
    private readonly Mock<IHistoryEventPublisher>? _historyPublisher;
    private readonly Mock<IRoleLabelService> _roleLabelService;
    private readonly Mock<ILogger<RolesService>>? _logger;
    private readonly Fixture _fixture;

    public RolesServiceTests()
    {
        _roleRepository = new Mock<IRoleRepository>();
        _contactRepository = new Mock<IContactRepository>();
        _rolePublisher = new Mock<IRoleEventPublisher>();
        _historyPublisher = new Mock<IHistoryEventPublisher>();
        _roleLabelService = new Mock<IRoleLabelService>();
        _logger = new Mock<ILogger<RolesService>>();

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
        var rolesService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        await roleService.CreateRoleAsync(createRoleRequest, contactId);

        // Assert
        _roleRepository.VerifyAll();
        _rolePublisher.Verify(x => x.PublishRoleCreatedEventAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        _historyPublisher.Verify(x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldThrow_BadRequestException()
    {
        // Arrange
        _roleRepository.Setup(repo => repo.CreateRoleAsync(null!))
            .ThrowsAsync(new BadRequestException(Errors.NotFoundAccountMessage, Errors.NotFoundAccountMessage));
        var contactId = 123;

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(accountId, request, currentUserId);

        // Assert
        result.Succeeded.Should().HaveCount(3);
        result.Succeeded.Select(s => s.ContactId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(
            r => r.CreateRoleWithoutAccountValidationAsync(It.Is<CreateRoleRequest>(x => x.AccountId == accountId)),
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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(accountId, request, currentUserId);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 1);
        result.Failed.Should().ContainSingle(f => f.ContactId == 2 && f.ErrorCode == Errors.BadRequestExistingRoleCode);
        _roleRepository.Verify(r => r.CreateRoleWithoutAccountValidationAsync(It.IsAny<CreateRoleRequest>()), Times.Exactly(2));
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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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
        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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
        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 8);
        _roleLabelService.Verify(r => r.AssignRoleLabelFromCodeAsync("CLP", 10, 8), Times.Once);
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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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
            .ThrowsAsync(new Exception("boom"));

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(_roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        var result = await roleService.CreateRolesBulkAsync(10, request, 25);

        // Assert
        result.Succeeded.Should().ContainSingle(s => s.ContactId == 24);
        result.Failed.Should().BeEmpty();
        _roleRepository.Verify(repo => repo.CreateRoleWithoutAccountValidationAsync(
            It.Is<CreateRoleRequest>(r => r.ContactId == 24 && r.IsCustomerRelation == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateRole_Should_ReturnsOkResultAsync()
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

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        await roleService.UpdateRoleSignatoryAsync(accountId, contactId, true);

        // Assert
        _roleRepository.VerifyAll();
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

        return roleRepository;
    }

    [Fact]
    public void DeleteRoleAsync_ShouldDeleteRole()
    {
        // Arrange
        var currentUserId = 25;
        var roleRepository = DeleteRole_MockRepo();
        _contactRepository.Setup(repo => repo.GetContactByIdAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<Contact>());

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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

        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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
        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

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
        var roleService = new RolesService(roleRepository.Object, _contactRepository.Object, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        Task DeleteRole() => roleService.DeleteRoleAsync(currentUserId, 1, 3);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
        _rolePublisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task CheckRoleExistsAsync_ReturnsOkResultAsync()
    {
        // Arrange
        _roleRepository.Setup(repository => repository.CheckRoleExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
        var rolesService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        var contactHasRoleOnAccount = await rolesService.CheckRoleExistsAsync(2, 1, 1, "test@test.fr");

        // Assert
        _roleRepository.Verify(x => x.CheckRoleExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task IsContactHasRoleInAccount_ReturnsTrueAsync()
    {
        // Arrange
        _roleRepository.Setup(repository => repository.IsContactHasRoleOnAccount(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
        var rolesService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        var contactHasRoleOnAccount = await rolesService.IsContactHasRoleOnAccount(1, 1, "accountNumber");

        // Assert
        _roleRepository.Verify(x => x.IsContactHasRoleOnAccount(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()));
        Assert.True(contactHasRoleOnAccount);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsync_Should_Return_Updated_Role()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        bool isCustomerRelation = true;

        var existingRole = new Role
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = false, // Different from what we're updating to
            IsFavorite = false,
            IsSignatory = false
        };

        var updatedRole = new Role
        {
            AccountId = accountId,
            ContactId = contactId,
            IsCustomerRelation = true, // Updated value
            IsFavorite = false,
            IsSignatory = false
        };

        _roleRepository.Setup(repo => repo.UpdateRoleCollaboratorInformationAsync(
                It.Is<int>(a => a == accountId),
                It.Is<int>(c => c == contactId),
                It.Is<bool>(r => r == isCustomerRelation),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        _rolePublisher!.Setup(x => x.PublishRoleUpdatedEventAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        await roleService.UpdateRoleCustomerRelationAsync(accountId, contactId, isCustomerRelation);

        // Assert
        _roleRepository.Verify(x => x.UpdateRoleCollaboratorInformationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
        _rolePublisher.Verify(x => x.PublishRoleUpdatedEventAsync(accountId, contactId), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleRelationClientAsync_Should_Throw_NotFoundException_When_Role_Does_Not_Exist()
    {
        // Arrange
        int accountId = 10;
        int contactId = 25;
        bool isCustomerRelation = true;

        _roleRepository.Setup(repo => repo.UpdateRoleCollaboratorInformationAsync(
                It.Is<int>(a => a == accountId),
                It.Is<int>(c => c == contactId),
                It.Is<bool>(r => r == isCustomerRelation),
                It.IsAny<int>()))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundRoleCode, Errors.NotFoundRoleMessage))
            .Verifiable();

        var roleService = new RolesService(_roleRepository.Object, null!, _rolePublisher!.Object, _historyPublisher!.Object, _roleLabelService.Object, _logger!.Object);

        // Act
        Func<Task> act = async () => await roleService.UpdateRoleCustomerRelationAsync(accountId, contactId, isCustomerRelation);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(Errors.NotFoundRoleMessage);
        _roleRepository.VerifyAll();
    }
}
