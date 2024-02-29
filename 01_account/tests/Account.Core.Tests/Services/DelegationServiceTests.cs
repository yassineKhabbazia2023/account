// <copyright file="DelegationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Moq;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services;

public class DelegationServiceTest
{
    private readonly Mock<IDelegationRepository> _repository;
    private readonly Fixture _fixture;

    public DelegationServiceTest()
    {
        _repository = new Mock<IDelegationRepository>(MockBehavior.Strict);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Request_IsValide_Should_Create_Delegation()
    {
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .Create();

        _repository.Setup(x => x.CreateDelegationAsync(createDelegation))
            .Callback<CreateDelegationRequest>(request =>
            {
                request.StartDate.Should().Be(createDelegation.StartDate);
                request.EndDate.Should().Be(createDelegation.EndDate);
                request.AccountId.Should().Be(createDelegation.AccountId);
                request.DelegatorId.Should().Be(createDelegation.DelegatorId);
                request.DelegateeId.Should().Be(createDelegation.DelegateeId);
            })
            .ReturnsAsync(100)
            .Verifiable();

        var service = new DelegationService(_repository.Object);
        var id = await service.CreateDelegationAsync(createDelegation);

        id.Should().Be(100);
        _repository.VerifyAll();
    }

    [Fact]
    public void CreateDelegationAsync_InvalidEndDate_ThrowException()
    {
        // Arrange
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(-1))
            .Create();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);
        var service = new DelegationService(repository.Object);

        // Act
        var act = async () => await service.CreateDelegationAsync(createDelegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.DelegationEndDateInvalidMessage, exception.Result.Message);
        Assert.Equal(Errors.DelegationEndDateInvalidCode, exception.Result.Code);
    }

    [Fact]
    public void CreateDelegationAsync_InvalidStartDate_ThrowException()
    {
        // Arrange
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .Without(p => p.StartDate)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(-1))
            .Create();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);
        var service = new DelegationService(repository.Object);

        // Act
        var act = async () => await service.CreateDelegationAsync(createDelegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.DelegationStartDateInvalidMessage, exception.Result.Message);
        Assert.Equal(Errors.DelegationStartDateInvalidCode, exception.Result.Code);
    }

    [Fact]
    public async Task CreateDelegationAsync_EndDateNull_ReturnOk()
    {
        // Arrange
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .Without(p => p.EndDate)
            .Create();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);
        repository.Setup(x => x.CreateDelegationAsync(createDelegation))
        .Callback<CreateDelegationRequest>(request =>
        {
            request.StartDate.Should().Be(createDelegation.StartDate);
            request.EndDate.Should().Be(createDelegation.EndDate);
            request.AccountId.Should().Be(createDelegation.AccountId);
            request.DelegatorId.Should().Be(createDelegation.DelegatorId);
            request.DelegateeId.Should().Be(createDelegation.DelegateeId);
        })
        .ReturnsAsync(100)
        .Verifiable();
        var service = new DelegationService(repository.Object);

        // Act
        var id = await service.CreateDelegationAsync(createDelegation);

        // Assert
        id.Should().Be(100);
    }

    [Fact]
    public async Task GetContactDelegationsAsync_Should_Return_ContactDelegations()
    {
        var contactId = 100;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();

        _repository.Setup(x => x.GetContactDelegationsAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(contactId))
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(_repository.Object);
        var contactDelegations = await service.GetContactDelegationsAsync(contactId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegationlist);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetDelegationsAsync_Should_Return_Delegations()
    {
        var delegatorId = 100;
        var delegateeId = 200;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();

        _repository.Setup(x => x.GetDelegationsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Callback<int, int>((sourceId, destinationId) =>
            {
                sourceId.Should().Be(delegatorId);
                destinationId.Should().Be(delegateeId);
            })
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(_repository.Object);
        var contactDelegations = await service.GetDelegationsAsync(delegatorId, delegateeId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegationlist);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task DeleteDelegationAsync_Nominal()
    {
        _repository.Setup(x => x.DeleteDelegationAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var service = new DelegationService(_repository.Object);
        await service.DeleteDelegationAsync(It.IsAny<int>());

        _repository.Verify(x => x.DeleteDelegationAsync(It.IsAny<int>()), Times.Once);
    }
}
