// <copyright file="DelegationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Kpmg.ExceptionMiddleware.AdvancedException;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
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
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegation()
    {
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();

        _repository.Setup(x => x.CreateDelegationAsync(createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
            .Callback<CreateDelegationRequest, IEnumerable<CreateRoleRequest>>((request, roles) =>
            {
                request.DelegationDetails.FirstOrDefault() !.StartDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.StartDate);
                request.DelegationDetails.FirstOrDefault() !.EndDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.EndDate);
                request.DelegatorId.Should().Be(createDelegation.DelegatorId);
            })
            .ReturnsAsync(100)
            .Verifiable();

        var service = new DelegationService(_repository.Object);
        var delegationId = await service.CreateDelegationAsync(createDelegation);

        delegationId.Should().Be(100);
        _repository.VerifyAll();
    }

    [Fact]
    public void CreateDelegationAsync_WithNullRequest_ShouldThrowBadRequestException()
    {
        var service = new DelegationService(null!);

        // Act
        var act = async () => await service.CreateDelegationAsync(null!);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.CreateDelegationMessage, exception.Result.Message);
        Assert.Equal(Errors.CreateDelegationCode, exception.Result.Code);
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
        var service = new DelegationService(repository.Object);

        // Act
        var act = async () => await service.CreateDelegationAsync(createDelegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(Errors.DelegationEndDateInvalidMessage, exception.Result.Message);
        Assert.Equal(Errors.DelegationEndDateInvalidCode, exception.Result.Code);
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenEndDateIsNull_ShouldReturnCreateDelegation()
    {
        // Arrange
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .Without(p => p.EndDate)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();
        var repository = new Mock<IDelegationRepository>(MockBehavior.Strict);
        repository.Setup(x => x.CreateDelegationAsync(createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
        .Callback<CreateDelegationRequest, IEnumerable<CreateRoleRequest>>((request, roles) =>
        {
            request.DelegationDetails.FirstOrDefault() !.StartDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.StartDate);
            request.DelegationDetails.FirstOrDefault() !.EndDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.EndDate);
            request.DelegatorId.Should().Be(createDelegation.DelegatorId);
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
    public async Task GetContactDelegationsAsync_WhenContactIdIsValid_ShouldReturnContactDelegations()
    {
        var contactId = 100;
        IReadOnlyCollection<Delegation> delegations = _fixture.Create<List<Delegation>>();

        _repository.Setup(x => x.GetContactDelegationsAsync(It.IsAny<int>()))
            .Callback<int>(id => id.Should().Be(contactId))
            .ReturnsAsync(delegations)
            .Verifiable();

        var service = new DelegationService(_repository.Object);
        var contactDelegations = await service.GetContactDelegationsAsync(contactId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegations);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetDelegationsAsync_WhenRequestIsValid_ShouldReturnDelegations()
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
    public async Task DeleteDelegationAsync_WhenDelegationIdIsValid_ShouldDeleteDelegation()
    {
        _repository.Setup(x => x.DeleteDelegationAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var service = new DelegationService(_repository.Object);
        await service.DeleteDelegationAsync(1);

        _repository.Verify(x => x.DeleteDelegationAsync(It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsNegativeOrNull_ShouldThrowBadRequestException(int delegationId)
    {
        var service = new DelegationService(null!);

        var result = await Assert.ThrowsAsync<BadRequestException>(async () => await service.DeleteDelegationAsync(delegationId));

        Assert.Equal(Errors.BadRequestDeleteDelegationCode, result.Code);
        Assert.Equal(Errors.BadRequestDeleteDelegationMessage, result.Message);
    }

    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_WhenAccountIdIsValid_ShouldReturnAccountDelegationsHistory()
    {
        // Arrange
        var accountId = 100;
        IReadOnlyCollection<Delegation> delegationlist = _fixture.Create<List<Delegation>>();

        _repository.Setup(x => x.DoesAccountExistAsync(It.IsAny<int>()))
          .Callback<int>((id) =>
          {
              id.Should().Be(accountId);
          })
          .ReturnsAsync(true)
          .Verifiable();

        _repository.Setup(x => x.GetAccountDelegationsHistoryAsync(It.IsAny<int>()))
            .Callback<int>((id) =>
            {
                id.Should().Be(accountId);
            })
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(_repository.Object);

        // Act
        var delegationsHistory = await service.GetAccountDelegationsHistoryAsync(accountId);

        // Assert
        delegationsHistory.Should().NotBeNull();
        delegationsHistory.Should().BeEquivalentTo(delegationlist);
        _repository.VerifyAll();
    }

    [Fact]
    public void GetAccountDelegationsHistoryAsync_WhenAccountIdIsInvalid_ShouldThrowException()
    {
        // Arrange
        var accountId = 100;

        _repository.Setup(x => x.DoesAccountExistAsync(It.IsAny<int>()))
            .Callback<int>((id) =>
            {
                id.Should().Be(accountId);
            })
            .ReturnsAsync(false)
            .Verifiable();

        var service = new DelegationService(_repository.Object);

        // Act
        var act = async () => await service.GetAccountDelegationsHistoryAsync(accountId);

        // Assert
        var exception = Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundAccountCode, exception.Result.Code);
        Assert.Equal(string.Format(Errors.NotFoundAccountMessage, accountId), exception.Result.Message);
    }

    [Fact]
    public void CreateRoleRequests_ShouldCreateRoleRequestList()
    {
        var details = _fixture.Build<DelegationDetails>()
            .With(x => x.IsRoleToCreate, true)
            .CreateMany(1);
        var accounts = new List<int> { 1 };
        var expected = _fixture.Build<CreateDelegationRequest>()
            .With(x => x.DelegationDetails, details)
            .With(x => x.AccountIds, accounts)
            .Create();

        var result = DelegationService.CreateRoleRequests(expected);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var resultItem = result.First();
        resultItem.AccountId.Should().Be(expected.AccountIds.First());
        resultItem.ContactId.Should().Be(expected.DelegationDetails.First().DelegateeId);
        resultItem.IsFavorite.Should().BeFalse();
        resultItem.IsSignatory.Should().BeFalse();
        resultItem.IsDelegation.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(DelegationRequestData))]
    public void CreateRoleRequestsWithNullOrEmtpyDelegationDetail_ShouldReturnEmptyList(CreateDelegationRequest delegation)
    {
        var result = DelegationService.CreateRoleRequests(delegation);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public static IEnumerable<object[]> DelegationRequestData => new List<object[]>
        {
            new object[] { null! },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = null!,
                    AccountIds = null!,
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = Enumerable.Empty<DelegationDetails>(),
                    AccountIds = null!,
                }
            },
        };
}
