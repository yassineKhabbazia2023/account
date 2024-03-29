// <copyright file="RolesServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Broker.Events;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Core.Tests.Services;

public class RolesServiceTests
{
    private readonly Mock<IServicePublisher>? _servicePublisher;
    private readonly Mock<ILogger<RolesService>>? _logger;

    public RolesServiceTests()
    {
        _servicePublisher = new Mock<IServicePublisher>();
        _logger = new Mock<ILogger<RolesService>>();
    }

    [Fact]
    public async Task GetContactRolesAsync_Should_ReturnsOkResultAsync()
    {
        // Arrange
        var fixture = new Fixture();
        var accountList = fixture.Create<Paging<AccountModel>>();
        var rolesRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        rolesRepository.Setup(repository => repository.GetContactRolesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(accountList);
        var rolesService = new RolesService(rolesRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        var accounts = await rolesService.GetContactRolesAsync(contactId: 123, pageNumber: 0, pageSize: 0);

        // Assert
        Assert.Equal(accountList, accounts);
        rolesRepository.Verify(x => x.GetContactRolesAsync(123, 1, int.MaxValue));
    }

    [Fact]
    public async Task GetSignatoryAsync_Should_Returns_Account_Signatory()
    {
        // Arrange
        var accountId = 116;
        var fixture = new Fixture();
        var expected = new List<Contact> { fixture.Create<Contact>() };

        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(accountId))
        .ReturnsAsync(expected);

        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        var result = await roleService.GetSignatoryAsync(accountId);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateRole_Should_ReturnsCreatedResultAsync()
    {
        // Arrange
        var newRoleId = 56;
        var createRoleRequest = new CreateRoleRequest()
        {
            AccountId = 6,
            ContactId = 6,
            IsFavorite = false,
            IsSignatory = false
        };

        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.CreateRoleAsync(It.IsAny<CreateRoleRequest>()))
            .ReturnsAsync(newRoleId)
            .Verifiable();

        _servicePublisher!.Setup(x => x.PublishAsync(It.IsAny<BaseEvent>()))
         .Callback<BaseEvent>(@event =>
         {
             var roleCreatedEvent = @event as RoleCreatedEvent;
             roleCreatedEvent.Should().NotBeNull();
             roleCreatedEvent!.DataEvent!.RoleId.Should().Be(newRoleId);
             roleCreatedEvent!.DataEvent!.ContactId.Should().Be(createRoleRequest.ContactId);
             roleCreatedEvent!.DataEvent!.AccountId.Should().Be(createRoleRequest.AccountId);
             roleCreatedEvent!.DataEvent!.IsSignatory.Should().Be(createRoleRequest.IsSignatory);
         })
         .Returns(Task.CompletedTask)
         .Verifiable();

        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        await roleService.CreateRoleAsync(createRoleRequest);

        // Assert
        roleRepository.VerifyAll();
        _servicePublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldThrow_BadRequestException()
    {
        // Arrange
        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.CreateRoleAsync(null!))
            .ThrowsAsync(new BadRequestException(Errors.NotFoundAccountMessage, Errors.NotFoundAccountMessage));

        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        Task Roles() => roleService.CreateRoleAsync(null!);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(Roles);
        _servicePublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRole_Should_ReturnsOkResultAsync()
    {
        // Arrange
        int roleId = 101;
        int accountId = 10;
        int contactId = 25;
        var roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        roleRepository.Setup(repo => repo.UpdateRoleSignatoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(roleId)
            .Verifiable();

        _servicePublisher!.Setup(x => x.PublishAsync(It.IsAny<BaseEvent>()))
          .Callback<BaseEvent>(@event =>
          {
              var roleUpdatedEvent = @event as RoleUpdatedEvent;
              roleUpdatedEvent.Should().NotBeNull();
              roleUpdatedEvent!.DataEvent!.RoleId.Should().Be(roleId);
              roleUpdatedEvent!.DataEvent!.ContactId.Should().Be(contactId);
              roleUpdatedEvent!.DataEvent!.AccountId.Should().Be(accountId);
              roleUpdatedEvent!.DataEvent!.IsSignatory.Should().Be(true);
          })
          .Returns(Task.CompletedTask)
          .Verifiable();

        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        await roleService.UpdateRoleSignatoryAsync(accountId, contactId, true);

        // Assert
        roleRepository.VerifyAll();
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
        roleRepository.Setup(repo => repo.DeleteRoleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(1);
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
        var roleRepository = DeleteRole_MockRepo();
        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _servicePublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent>()), Times.Once);
    }

    [Fact]
    public void DeleteRoleAsync_ShouldDeleteRole_Cas2Signatory()
    {
        // Arrange
        var roleRepository = DeleteRole_MockRepo();
        var fixture = new Fixture();
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .ReturnsAsync(fixture.CreateMany<Contact>(2));
        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        Task DeleteRole() => roleService!.DeleteRoleAsync(1, 1);

        // Assert
        Assert.Equal(Task.CompletedTask, DeleteRole());
        _servicePublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrow_NotFoundException()
    {
        // Arrange
        var roleRepository = DeleteRole_MockRepo();
        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        Task DeleteRole() => roleService.DeleteRoleAsync(1, 2);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(DeleteRole);
        _servicePublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrow_BadRequestException()
    {
        // Arrange
        var roleRepository = DeleteRole_MockRepo();
        var fixture = new Fixture();
        roleRepository.Setup(repo => repo.GetSignatoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contact> { fixture.Create<Contact>() });
        var roleService = new RolesService(roleRepository.Object, _servicePublisher!.Object, _logger!.Object);

        // Act
        Task DeleteRole() => roleService.DeleteRoleAsync(1, 3);

        // Assert
        await Assert.ThrowsAsync<BadRequestException>(DeleteRole);
        _servicePublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent>()), Times.Never);
    }
}
