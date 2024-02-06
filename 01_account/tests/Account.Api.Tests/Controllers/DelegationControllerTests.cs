// <copyright file="DelegationController.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Account.Api.Tests.Controllers;

public class DelegationControllerTests
{
    private readonly Fixture _fixture;

    public DelegationControllerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Request_IsValide_Should_Create_Delegation()
    {
        var createDelegation = _fixture.Create<CreateDelegation>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.CreateDelegationAsync(createDelegation))
            .Callback<CreateDelegation>(request =>
            {
                request.StartDate.Should().Be(createDelegation.StartDate);
                request.EndDate.Should().Be(createDelegation.EndDate);
                request.GlobalAccountId.Should().Be(createDelegation.GlobalAccountId);
                request.GlobalDelegatorId.Should().Be(createDelegation.GlobalDelegatorId);
                request.GlobalDelegateeId.Should().Be(createDelegation.GlobalDelegateeId);
            })
            .ReturnsAsync(100)
            .Verifiable();

        var controller = new DelegationController(service.Object);
        var actionResult = await controller.CreateDelegationAsync(createDelegation);

        actionResult.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.As<OkObjectResult>().Value.Should().Be(100);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetContactDelegationsAsync_Should_Return_ContactDelegations()
    {
        var contactId = Guid.Parse("11111111-1111-1111-1111-000000000000");
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetContactDelegationsAsync(It.IsAny<Guid>()))
            .Callback<Guid>(id => id.Should().Be(contactId))
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var controller = new DelegationController(service.Object);
        var actionResult = await controller.GetContactDelegationsAsync(contactId);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(delegationlist);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetDelegationsAsync_Should_Return_Delegations()
    {
        var delegatorId = Guid.Parse("11111111-1111-1111-0000-000000000000");
        var delegateeId = Guid.Parse("11111111-1111-1111-1111-000000000000");
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetDelegationsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Callback<Guid, Guid>((sourceId, destinationId) =>
            {
                sourceId.Should().Be(delegatorId);
                destinationId.Should().Be(delegateeId);
            })
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var controller = new DelegationController(service.Object);
        var actionResult = await controller.GetDelegationsAsync(delegatorId, delegateeId);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(delegationlist);
        service.VerifyAll();
    }
}
