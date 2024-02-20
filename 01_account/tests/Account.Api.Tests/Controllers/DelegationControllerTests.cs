// <copyright file="DelegationControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
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
                request.AccountId.Should().Be(createDelegation.AccountId);
                request.DelegatorId.Should().Be(createDelegation.DelegatorId);
                request.DelegateeId.Should().Be(createDelegation.DelegateeId);
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
        var contactId = 100;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetContactDelegationsAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(contactId))
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
        var delegatorId = 100;
        var delegateeId = 200;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();
        var service = new Mock<IDelegationService>(MockBehavior.Strict);

        service.Setup(x => x.GetDelegationsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Callback<int, int>((sourceId, destinationId) =>
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
