// <copyright file="DelegationControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Account.Api.Tests.Controllers;

public class DelegationControllerTests
{
    private readonly Mock<IDelegationService> _service;
    private readonly Fixture _fixture;

    public DelegationControllerTests()
    {
        _service = new Mock<IDelegationService>(MockBehavior.Strict);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegation()
    {
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();

        _service.Setup(x => x.CreateDelegationAsync(createDelegation))
            .Callback<CreateDelegationRequest>(request =>
            {
                request.DelegationDetails.FirstOrDefault() !.StartDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.StartDate);
                request.DelegationDetails.FirstOrDefault() !.EndDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.EndDate);
                request.DelegatorId.Should().Be(createDelegation.DelegatorId);
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        var controller = new DelegationController(_service.Object);
        var actionResult = await controller.CreateDelegationAsync(createDelegation);

        actionResult.As<OkResult>().StatusCode.Should().Be(200);
        _service.VerifyAll();
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenEndDateIsNull_ShouldCreateDelegation()
    {
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .Without(p => p.EndDate)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.CreateDelegationAsync(createDelegation))
            .Callback<CreateDelegationRequest>(request =>
            {
                request.DelegationDetails.FirstOrDefault() !.StartDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.StartDate);
                request.DelegationDetails.FirstOrDefault() !.EndDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.EndDate);
                request.DelegatorId.Should().Be(createDelegation.DelegatorId);
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        var controller = new DelegationController(service.Object);
        var actionResult = await controller.CreateDelegationAsync(createDelegation);

        actionResult.As<OkResult>().StatusCode.Should().Be(200);
        service.VerifyAll();
    }

    [Fact]
    public void CreateDelegationAsync_WhenEndDateIsInvalid_ShouldThrowException()
    {
        // Arrange
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(-1))
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);
        repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);
        var service = new DelegationService(repository.Object, null!, null!);

        var controller = new DelegationController(service);

        // Act
        var act = async () => await controller.CreateDelegationAsync(createDelegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.DelegationEndDateInvalidMessage, exception.Result.Message);
        Assert.Equal(Errors.DelegationEndDateInvalidCode, exception.Result.Code);
    }

    [Fact]
    public async Task GetContactDelegationsAsync_WhenContactIdIsValid_ShouldReturnContactDelegations()
    {
        var contactId = 100;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();

        _service.Setup(x => x.GetContactDelegationsAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(contactId))
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var controller = new DelegationController(_service.Object);
        var actionResult = await controller.GetContactDelegationsAsync(contactId);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(delegationlist);
        _service.VerifyAll();
    }

    [Fact]
    public async Task GetDelegationsAsync_WhenRequestIsValid_ShouldReturnDelegations()
    {
        var delegatorId = 100;
        var delegateeId = 200;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();

        _service.Setup(x => x.GetDelegationsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Callback<int, int>((sourceId, destinationId) =>
            {
                sourceId.Should().Be(delegatorId);
                destinationId.Should().Be(delegateeId);
            })
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var controller = new DelegationController(_service.Object);
        var actionResult = await controller.GetDelegationsAsync(delegatorId, delegateeId);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(delegationlist);
        _service.VerifyAll();
    }

    [Fact]
    public async Task DeleteDelegationAsync_WenDelegationIdIsValid_ShouldDeleteDelegation()
    {
        _service.Setup(x => x.DeleteDelegationAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var controller = new DelegationController(_service.Object);

        var result = await controller.DeleteDelegationAsync(It.IsAny<int>());

        result.As<OkResult>().StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsInvalid_ShouldThrowException()
    {
        var exception = new NotFoundException(It.IsAny<string>(), It.IsAny<string>());
        _service.Setup(x => x.DeleteDelegationAsync(It.IsAny<int>())).ThrowsAsync(exception);

        var controller = new DelegationController(_service.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await controller.DeleteDelegationAsync(It.IsAny<int>()));

        result.Code.Should().Be(exception.Code);
        result.Message.Should().Be(exception.Message);
    }

    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_WhenAccountIdIsValid_ShouldReturnAccountDelegationsHistory()
    {
        // Arrange
        var accountId = 100;
        var delegations = _fixture.Create<Paging<Delegation>>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetAccountDelegationsHistoryAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Pagination>()))
            .Callback<int, string, Pagination>((id, search, pagination) =>
            {
                id.Should().Be(accountId);
            })
            .ReturnsAsync(delegations)
            .Verifiable();

        var controller = new DelegationController(service.Object);

        // Act
        var actionResult = await controller.GetAccountDelegationsHistoryAsync(accountId, null, new Pagination());

        // Assert
        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(delegations);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetContactDelegationsHistoryAsync_ShouldReturnDelegation_WhenContactValid()
    {
        // Arrange
        var contactId = 123;
        var delegations = _fixture.Create<Paging<Delegation>>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetContactDelegationsHistoryAsync(contactId, It.IsAny<Pagination>(), It.IsAny<bool>()))
            .ReturnsAsync(delegations)
            .Verifiable();

        var controller = new DelegationController(service.Object);

        // Act
        var result = await controller.GetContactDelegationsHistoryAsync(contactId, new Pagination(), true);

        // Assert
        result.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        result.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(delegations);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetContactDelegationsHistoryAsync_ShouldReturnNotFound_WhenContactNotFound()
    {
        // Arrange
        var contactId = 123;
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetContactDelegationsHistoryAsync(contactId, It.IsAny<Pagination>(), It.IsAny<bool>()))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId)))
            .Verifiable();

        var controller = new DelegationController(service.Object);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await controller.GetContactDelegationsHistoryAsync(contactId, new Pagination(), true));

        // Assert
        service.VerifyAll();
    }
}
