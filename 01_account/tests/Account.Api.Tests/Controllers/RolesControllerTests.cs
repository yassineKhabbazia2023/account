// <copyright file="RolesControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Account.Api.Tests.Controllers;

public class RolesControllerTests
{
    private Fixture _fixture;

    public RolesControllerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetContactRoles_Should_ReturnsOkResultAsync()
    {
        // Arrange
        var pagination = new Pagination
        {
            PageNumber = 1,
            PageSize = 4
        };
        var accountMocked = _fixture.Create<Paging<Pulse.Account.Core.Models.Account>>();
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(service => service.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<Pagination>())).ReturnsAsync(accountMocked);

        var rolesController = new RolesController(rolesService.Object);

        // Act
        var accounts = await rolesController.GetContactRolesAsync(contactId: 123, pagination);
        var resultAccounts = accounts?.Result as OkObjectResult;

        // Assert
        Assert.Equal(accountMocked, resultAccounts?.Value);
    }

    [Fact]
    public async Task GetSignatoryAsync_Should_Returns_Account_Signatory()
    {
        // Arrange
        var accountId = 116;
        var fixture = new Fixture();
        var expected = new List<Contact> { fixture.Create<Contact>() };

        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(service => service.GetSignatoryAsync(It.IsAny<int>()))
            .ReturnsAsync(expected);
        var rolesController = new RolesController(rolesService.Object);

        // Act
        var result = await rolesController.GetSignatoryAsync(accountId);

        // Assert
        Assert.Equal(expected, (result.Result as OkObjectResult)?.Value);
    }

    [Fact]
    public async Task CreateRole_Should_ReturnCreatedResultAsync()
    {
        // Arrange
        var mockRoleService = new Mock<IRolesService>(MockBehavior.Strict);
        var roleRequest = new CreateRoleRequest
        {
            AccountId = 6,
            ContactId = 6,
            IsFavorite = false,
            IsSignatory = false
        };

        mockRoleService.Setup(service => service.CreateRoleAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        var rolesController = new RolesController(mockRoleService.Object);

        // Act
        var result = await rolesController.CreateRoleAsync(12, 6, roleRequest);

        // Assert
        mockRoleService.Verify(s => s.CreateRoleAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<int>()), Times.Once);
        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task CreateRole_Should_ReturnBadRequestExceptionAsync()
    {
        // Arrange
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(service => service.CreateRoleAsync(It.IsAny<CreateRoleRequest>(), It.IsAny<int>()))
            .Throws(new BadRequestException(Errors.BadRequestRoleCode, Errors.BadRequestRoleMessage));
        var rolesController = new RolesController(rolesService.Object);

        // Act
        Task Roles() => rolesController.CreateRoleAsync(123, 1, null!);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(Roles);
    }

    [Fact]
    public async Task CreateRolesAsync_Should_ReturnOkResultWithBulkResult()
    {
        // Arrange
        var expected = new CreateRolesBulkResult
        {
            Succeeded = new List<CreateRolesBulkItemResult> { new() { ContactId = 1 } },
            Failed = new List<CreateRolesBulkItemResult> { new() { ContactId = 2, ErrorCode = Errors.BadRequestExistingRoleCode } }
        };
        var request = new CreateRolesBulkRequest
        {
            Contacts = new List<CreateRolesBulkItem>
            {
                new() { ContactId = 1, IsSignatory = true },
                new() { ContactId = 2, IsSignatory = false }
            }
        };
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(s => s.CreateRolesBulkAsync(10, request, 25)).ReturnsAsync(expected);

        var rolesController = new RolesController(rolesService.Object);

        // Act
        var actionResult = await rolesController.CreateRolesBulkAsync(25, 10, request);
        var okResult = actionResult.Result as OkObjectResult;

        // Assert
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeSameAs(expected);
        rolesService.Verify(s => s.CreateRolesBulkAsync(10, request, 25), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleSignatoryAsync_Should_ReturnOkResult()
    {
        // Arrange
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(service => service.UpdateRoleSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        var rolesController = new RolesController(rolesService.Object);

        // Act
        var actionResult = await rolesController.UpdateRoleSignatoryAsync(2, 1, 1, true);
        var result = actionResult as StatusCodeResult;

        // Assert
        result!.StatusCode.Should().Be(200);
        rolesService.Verify(x => x.UpdateRoleSignatoryAsync(2, 1, 1, true), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_Should_ReturnOkResultWithBulkResult()
    {
        // Arrange
        var currentUserId = 25;
        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = true,
            AccountIds = new List<int> { 1, 2 }
        };
        var expected = new UpdateRoleCustomerRelationResponse
        {
            Succeeded = new List<UpdateRoleCustomerRelationItemResponse>
            {
                new() { AccountId = 1 },
                new() { AccountId = 2 }
            }
        };

        var contactType = "Collaborator";
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(service => service.BulkUpdateRoleCustomerRelationAsync(currentUserId, contactType, request))
            .ReturnsAsync(expected);
        var rolesController = new RolesController(rolesService.Object);

        // Act
        var actionResult = await rolesController.BulkUpdateRoleCustomerRelationAsync(currentUserId, contactType, request);
        var okResult = actionResult.Result as OkObjectResult;

        // Assert
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeSameAs(expected);
        rolesService.Verify(x => x.BulkUpdateRoleCustomerRelationAsync(currentUserId, contactType, request), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateRoleCustomerRelationAsync_Should_PropagateNotFoundException_WhenServiceThrows()
    {
        // Arrange
        var currentUserId = 25;
        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = true,
            AccountIds = new List<int> { 1 }
        };

        var contactType = "Collaborator";
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(service => service.BulkUpdateRoleCustomerRelationAsync(currentUserId, contactType, request))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundRoleCode, Errors.NotFoundRoleMessage));
        var rolesController = new RolesController(rolesService.Object);

        // Act
        Func<Task> act = async () => await rolesController.BulkUpdateRoleCustomerRelationAsync(currentUserId, contactType, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        rolesService.Verify(x => x.BulkUpdateRoleCustomerRelationAsync(currentUserId, contactType, request), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldDeleteRole()
    {
        // Arrange
        var currentUserId = 25;
        var roleService = new Mock<IRolesService>();
        roleService.Setup(service => service.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        var roleController = new RolesController(roleService.Object);

        // Act
        var result = await roleController.DeleteRoleAsync(currentUserId, 1, 1) as StatusCodeResult;

        // Assert
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrowNotFoundException()
    {
        // Arrange
        var currentUserId = 25;
        var roleService = new Mock<IRolesService>();
        roleService.Setup(service => service.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Throws(new NotFoundException(Errors.NotFoundRoleCode, Errors.NotFoundRoleMessage));
        var roleController = new RolesController(roleService.Object);

        // Act
        Task DeleteRole() => roleController!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(DeleteRole);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrowBadRequestException()
    {
        // Arrange
        var currentUserId = 25;
        var roleService = new Mock<IRolesService>();
        roleService.Setup(service => service.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Throws(new BadRequestException(Errors.CannotDeleteSignatoryCode, Errors.CannotDeleteSignatoryMessage));
        var roleController = new RolesController(roleService.Object);

        // Act
        Task DeleteRole() => roleController!.DeleteRoleAsync(currentUserId, 1, 1);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
    }

    [Fact]
    public async Task CheckRoleExists_Should_ReturnOkResultAsync()
    {
        // Arrange
        var roleService = new Mock<IRolesService>();
        roleService.Setup(service => service.CheckRoleExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), "test@test.fr"))
            .Returns(Task.FromResult(true));
        var roleController = new RolesController(roleService.Object);

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();

        var headers = new HeaderDictionary { { "CurrentUser", new StringValues("123") } };
        requestMock.Setup(r => r.Headers).Returns(headers);

        httpContextMock.Setup(ctx => ctx.Request).Returns(requestMock.Object);
        roleController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var contactHasRoleOnAccount = await roleController.CheckRoleExists(1, 1, "test@test.fr");

        // Assert
        roleService.Verify(x => x.CheckRoleExistsAsync(123, 1, 1, "test@test.fr"), Times.Once);
        Assert.Equal(true, (contactHasRoleOnAccount as OkObjectResult)?.Value);
    }

    [Theory]
    [MemberData(nameof(HeaderParams))]
    public async Task CheckRoleExists_WithInvalidCurrentUser_Should_ThrowBadRequestException(HeaderDictionary header, string code, string message)
    {
        var roleController = new RolesController(null!);

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();

        requestMock.Setup(r => r.Headers).Returns(header);

        httpContextMock.Setup(ctx => ctx.Request).Returns(requestMock.Object);
        roleController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        var result = await Assert.ThrowsAsync<BadRequestException>(async () => await roleController.CheckRoleExists(1, 1, "test@test.fr"));

        Assert.Equal(code, result.Code);
        Assert.Equal(message, result.Message);
    }

    public static IEnumerable<object[]> HeaderParams()
    {
        yield return new object[]
        {
            new HeaderDictionary(),
            "ACC029",
            "Le CurrentUser n'a pas été transmis via header."
        };

        yield return new object[]
        {
            new HeaderDictionary { { "CurrentUser", new StringValues("toto") } },
            "ACC030",
            "Le header CurrentUser doit être un entier valide."
        };
    }

    [Fact]
    public async Task CheckRoleExists_WithContactIdAndEmailNull_Should_ThrowBadRequestException()
    {
        var roleController = new RolesController(null!);

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();

        var header = new HeaderDictionary { { "CurrentUser", new StringValues("123") } };
        requestMock.Setup(r => r.Headers).Returns(header);

        httpContextMock.Setup(ctx => ctx.Request).Returns(requestMock.Object);
        roleController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        var result = await Assert.ThrowsAsync<BadRequestException>(async () => await roleController.CheckRoleExists(null!, 1, string.Empty));

        Assert.Equal("ACC031", result.Code);
        Assert.Equal("Veuillez fournir au moins le ContactId ou l'email.", result.Message);
    }

    [Fact]
    public async Task IsContactHasRoleOnAccount_Should_ReturnOkResultAsync()
    {
        // Arrange
        var roleService = new Mock<IRolesService>();
        roleService.Setup(service => service.IsContactHasRoleOnAccount(It.IsAny<int>(), It.IsAny<int>(), null))
            .Returns(Task.FromResult(true));
        var roleController = new RolesController(roleService.Object);

        // Act
        var contactHasRoleOnAccount = await roleController.IsContactHasRoleOnAccount(1, 1, null);

        // Assert
        roleService.Verify(x => x.IsContactHasRoleOnAccount(1, 1, null), Times.Once);
        Assert.Equal(true, (contactHasRoleOnAccount as OkObjectResult)?.Value);
    }

    [Fact]
    public async Task IsContactHasRoleOnAccount_ShouldThrowBadRequestException()
    {
        // Arrange
        var roleService = new Mock<IRolesService>();
        roleService.Setup(service => service.IsContactHasRoleOnAccount(It.IsAny<int>(), null, null))
            .Returns(Task.FromResult(true));
        var roleController = new RolesController(roleService.Object);

        // Act
        Task ContactHasRoleOnAccount() => roleController!.IsContactHasRoleOnAccount(1, null, null);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(ContactHasRoleOnAccount);
    }

    [Fact]
    public async Task BulkDeleteRolesAsync_Should_ReturnOkResultWithResult()
    {
        // Arrange
        var currentUserId = 25;
        var expected = new BulkRoleDeleteResult
        {
            Succeeded = new List<BulkRoleDeleteItemResult> { new() { AccountId = 1 } },
            Failed = new List<BulkRoleDeleteItemResult> { new() { AccountId = 2, ErrorCode = Errors.NotFoundRoleCode } },
        };
        var request = new BulkRoleDeleteRequest { AccountIds = new List<int> { 1, 2 } };
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(s => s.BulkDeleteRolesAsync(currentUserId, request)).ReturnsAsync(expected);

        var rolesController = new RolesController(rolesService.Object);

        // Act
        var actionResult = await rolesController.BulkDeleteRolesAsync(currentUserId, request);
        var okResult = actionResult.Result as OkObjectResult;

        // Assert
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeSameAs(expected);
        rolesService.Verify(s => s.BulkDeleteRolesAsync(currentUserId, request), Times.Once);
    }

    [Fact]
    public async Task CheckLastCollaboratorAsync_Should_ReturnOkResultWithResult()
    {
        // Arrange
        var contactId = 25;
        var accountIds = new List<int> { 1, 2 };
        var expected = new LastCollaboratorCheckResult
        {
            IsLastCollaboratorOnAny = true,
            AccountIdsWhereLastCollaborator = new List<int> { 1 },
        };
        var rolesService = new Mock<IRolesService>(MockBehavior.Strict);
        rolesService.Setup(s => s.CheckLastCollaboratorAsync(contactId, accountIds)).ReturnsAsync(expected);

        var rolesController = new RolesController(rolesService.Object);

        // Act
        var actionResult = await rolesController.CheckLastCollaboratorAsync(contactId, accountIds);
        var okResult = actionResult.Result as OkObjectResult;

        // Assert
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeSameAs(expected);
        rolesService.Verify(s => s.CheckLastCollaboratorAsync(contactId, accountIds), Times.Once);
    }
}
