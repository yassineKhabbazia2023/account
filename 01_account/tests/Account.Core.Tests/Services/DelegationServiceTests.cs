// <copyright file="DelegationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System;
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

namespace Pulse.Account.Core.Tests.Services;

public class DelegationServiceTest
{
    private readonly Mock<IDelegationRepository> _repository;
    private readonly Mock<IRoleEventPublisher> _publisher;
    private readonly Mock<ILogger<DelegationService>> _logger;
    private readonly Fixture _fixture;

    public DelegationServiceTest()
    {
        _repository = new Mock<IDelegationRepository>();
        _fixture = new Fixture();
        _publisher = new Mock<IRoleEventPublisher>();
        _logger = new Mock<ILogger<DelegationService>>();
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegation()
    {
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .With(p => p.IsRoleToCreate, true)
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
            });

        var service = new DelegationService(_repository.Object, _publisher.Object, _logger.Object);
        await service.CreateDelegationAsync(createDelegation);

        _repository.VerifyAll();
        _publisher.VerifyAll();
    }

    [Theory]
    [MemberData(nameof(CreateDelegationData))]
    public void CreateDelegationAsync_WithInvalidParameters_ShouldThrowBadRequestException(CreateDelegationRequest delegation)
    {
        var service = new DelegationService(null!, null!, null!);

        // Act
        var act = async () => await service.CreateDelegationAsync(delegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Impossible de créer une délégation : les informations fournies dans la requête sont incorrectes.", exception.Result.Message);
        Assert.Equal("ACC003", exception.Result.Code);
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

        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);

        var service = new DelegationService(_repository.Object, null!, null!);

        // Act
        var act = async () => await service.CreateDelegationAsync(createDelegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Impossible de créer une délégation : La date de début de la délégation ne peut pas être supérieur à la date de fin.", exception.Result.Message);
        Assert.Equal("ACC007", exception.Result.Code);
    }

    [Fact]
    public void CreateDelegationAsync_ShouldThrowBadRequestException_IfRequestContainsClient()
    {
        var createDelegation = _fixture.Create<CreateDelegationRequest>();
        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(true);

        var service = new DelegationService(_repository.Object, null!, null!);

        var act = async () => await service.CreateDelegationAsync(createDelegation);

        var result = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("ACC023", result.Result.Code);
        Assert.Equal("Un client ne peut pas émettre ou recevoir de délégation", result.Result.Message);
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

        var service = new DelegationService(_repository.Object, null!, null!);
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

        var service = new DelegationService(_repository.Object, null!, null!);
        var contactDelegations = await service.GetDelegationsAsync(delegatorId, delegateeId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegationlist);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsValid_ShouldDeleteDelegation()
    {
        var roleToDelete = new Role
        {
            AccountId = 1,
            ContactId = 1,
        };

        _repository.Setup(x => x.DeleteDelegationAsync(It.IsAny<int>())).ReturnsAsync(new List<Role> { roleToDelete });

        var service = new DelegationService(_repository.Object, _publisher.Object, _logger.Object);
        await service.DeleteDelegationAsync(1);

        _repository.Verify(x => x.DeleteDelegationAsync(It.IsAny<int>()), Times.Once);
        _publisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsNegativeOrNull_ShouldThrowBadRequestException(int delegationId)
    {
        var service = new DelegationService(null!, _publisher.Object, null!);

        var result = await Assert.ThrowsAsync<BadRequestException>(async () => await service.DeleteDelegationAsync(delegationId));

        Assert.Equal(Errors.BadRequestDeleteDelegationCode, result.Code);
        Assert.Equal(Errors.BadRequestDeleteDelegationMessage, result.Message);

        _publisher.Verify(p => p.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_WhenAccountIdIsValid_ShouldReturnAccountDelegationsHistory()
    {
        // Arrange
        var accountId = 100;
        var delegationlist = _fixture.Create<Paging<Delegation>>();

        _repository.Setup(x => x.DoesAccountExistAsync(It.IsAny<int>()))
          .Callback<int>((id) =>
          {
              id.Should().Be(accountId);
          })
          .ReturnsAsync(true)
          .Verifiable();

        _repository.Setup(x => x.GetAccountDelegationsHistoryAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Pagination>()))
            .Callback<int, string, Pagination>((id, search, pagination) =>
            {
                id.Should().Be(accountId);
            })
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(_repository.Object, null!, null!);

        // Act
        var delegationsHistory = await service.GetAccountDelegationsHistoryAsync(accountId, null!, null!);

        // Assert
        delegationsHistory.Should().NotBeNull();
        delegationsHistory.Should().BeEquivalentTo(delegationlist);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetContactDelegationsHistoryAsync_WhenContactIdIsValid_ShouldReturnContactDelegationsHistory()
    {
        // Arrange
        var contactId = 100;
        var delegationlist = _fixture.Create<Paging<Delegation>>();

        _repository.Setup(x => x.DoesContactExistAsync(It.IsAny<int>()))
          .Callback<int>((id) =>
          {
              id.Should().Be(contactId);
          })
          .ReturnsAsync(true)
          .Verifiable();

        _repository.Setup(x => x.GetContactDelegationsHistoryAsync(contactId, It.IsAny<Pagination>(), It.IsAny<bool>()))
            .ReturnsAsync(delegationlist)
            .Verifiable();

        var service = new DelegationService(_repository.Object, null!, null!);

        // Act
        var delegationsHistory = await service.GetContactDelegationsHistoryAsync(contactId, new Pagination(), true);

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

        var service = new DelegationService(_repository.Object, null!, null!);

        // Act
        var act = async () => await service.GetAccountDelegationsHistoryAsync(accountId, null!, null!);

        // Assert
        var exception = Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundAccountCode, exception.Result.Code);
        Assert.Equal(string.Format(Errors.NotFoundAccountMessage, accountId), exception.Result.Message);
    }

    [Fact]
    public void GetContactDelegationsHistoryAsync_WhenContactIdIsInvalid_ShouldThrowException()
    {
        // Arrange
        var contactId = 100;

        _repository.Setup(x => x.DoesContactExistAsync(contactId))
            .ReturnsAsync(false)
            .Verifiable();

        var service = new DelegationService(_repository.Object, null!, null!);

        // Act
        var act = async () => await service.GetContactDelegationsHistoryAsync(contactId, new Pagination(), false);

        // Assert
        var exception = Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Equal(Errors.NotFoundContactCode, exception.Result.Code);
        Assert.Equal(string.Format(Errors.NotFoundContactMessage, contactId), exception.Result.Message);
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
        resultItem.AccountId.Should().Be(expected.AccountIds!.First());
        resultItem.ContactId.Should().Be(expected.DelegationDetails.First().DelegateeId);
        resultItem.IsFavorite.Should().BeFalse();
        resultItem.IsSignatory.Should().BeFalse();
        resultItem.IsDelegation.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(CreateRoleRequestData))]
    public void CreateRoleRequestsWithNullOrEmtpyDelegationDetail_ShouldReturnEmptyList(CreateDelegationRequest delegation)
    {
        var result = DelegationService.CreateRoleRequests(delegation);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public static IEnumerable<object[]> CreateDelegationData => new List<object[]>
        {
            new object[] { null! },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = Enumerable.Empty<DelegationDetails>(),
                    AccountIds = null!,
                    IsFullDelegation = true
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = new List<DelegationDetails> { new DelegationDetails { DelegateeId = 1 } },
                    AccountIds = null!,
                    IsFullDelegation = false
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = new List<DelegationDetails> { new DelegationDetails { DelegateeId = 1 } },
                    AccountIds = Enumerable.Empty<int>(),
                    IsFullDelegation = false
                }
            }
        };

    public static IEnumerable<object[]> CreateRoleRequestData => new List<object[]>
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
