// <copyright file="DelegationServiceTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Moq;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services;

public class DelegationServiceTest
{
    private readonly Fixture _fixture;

    public DelegationServiceTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Request_IsValide_Should_Create_Delegation()
    {
        var createDelegation = _fixture.Create<CreateDelegation>();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);

        repository.Setup(x => x.CreateDelegationAsync(createDelegation))
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

        var service = new DelegationService(repository.Object);
        var id = await service.CreateDelegationAsync(createDelegation);

        id.Should().Be(100);
        repository.VerifyAll();
    }

    [Fact]
    public async Task GetContactDelegationsAsync_Should_Return_ContactDelegations()
    {
        var contactId = Guid.Parse("11111111-1111-1111-1111-000000000000");
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);

        repository.Setup(x => x.GetContactDelegationsAsync(It.IsAny<Guid>()))
            .Callback<Guid>(id => id.Should().Be(contactId))
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(repository.Object);
        var contactDelegations = await service.GetContactDelegationsAsync(contactId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegationlist);
        repository.VerifyAll();
    }

    [Fact]
    public async Task GetDelegationsAsync_Should_Return_Delegations()
    {
        var delegatorId = Guid.Parse("11111111-1111-1111-0000-000000000000");
        var delegateeId = Guid.Parse("11111111-1111-1111-1111-000000000000");
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);

        repository.Setup(x => x.GetDelegationsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Callback<Guid, Guid>((sourceId, destinationId) =>
            {
                sourceId.Should().Be(delegatorId);
                destinationId.Should().Be(delegateeId);
            })
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(repository.Object);
        var contactDelegations = await service.GetDelegationsAsync(delegatorId, delegateeId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegationlist);
        repository.VerifyAll();
    }
}
