// <copyright file="DelegationServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Enum;
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
    private readonly Mock<IHistoryEventPublisher> _historyPublisher;
    private readonly Mock<ILogger<DelegationService>> _logger;
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Fixture _fixture;

    public DelegationServiceTest()
    {
        _repository = new Mock<IDelegationRepository>();
        _fixture = new Fixture();
        _publisher = new Mock<IRoleEventPublisher>();
        _historyPublisher = new Mock<IHistoryEventPublisher>();
        _logger = new Mock<ILogger<DelegationService>>();
        _roleRepository = new Mock<IRoleRepository>();
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRequestIsValid_ShouldCreateDelegation()
    {
        var contactId = 25;
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .With(p => p.IsRoleToCreate, true)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();

        _repository.Setup(x => x.CreateDelegationAsync(contactId, createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
            .Callback<int, CreateDelegationRequest, IEnumerable<CreateRoleRequest>>((id, request, roles) =>
            {
                request.DelegationDetails.FirstOrDefault() !.StartDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.StartDate);
                request.DelegationDetails.FirstOrDefault() !.EndDate.Should().Be(createDelegation.DelegationDetails.FirstOrDefault() !.EndDate);
            });

        var service = new DelegationService(_repository.Object, _publisher.Object, _historyPublisher.Object, _logger.Object, _roleRepository.Object);
        await service.CreateDelegationAsync(contactId, createDelegation);

        _repository.VerifyAll();
        _publisher.VerifyAll();
    }

    [Theory]
    [MemberData(nameof(CreateDelegationData))]
    public void CreateDelegationAsync_WithInvalidParameters_ShouldThrowBadRequestException(CreateDelegationRequest delegation)
    {
        var contactId = 25;
        var service = new DelegationService(null!, null!, null!, null!, null!);

        // Act
        var act = async () => await service.CreateDelegationAsync(contactId, delegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Impossible de créer une délégation : les informations fournies dans la requête sont incorrectes.", exception.Result.Message);
        Assert.Equal("ACC003", exception.Result.Code);
    }

    [Fact]
    public void CreateDelegationAsync_WhenEndDateIsInvalid_ShouldThrowException()
    {
        // Arrange
        var contactId = 25;
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(-1))
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .Create();

        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);

        // Act
        var act = async () => await service.CreateDelegationAsync(contactId, createDelegation);

        // Assert
        var exception = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("Impossible de créer une délégation : La date de début de la délégation ne peut pas être supérieur à la date de fin.", exception.Result.Message);
        Assert.Equal("ACC007", exception.Result.Code);
    }

    [Fact]
    public void CreateDelegationAsync_ShouldThrowBadRequestException_IfRequestContainsClient()
    {
        var contactId = 25;
        var createDelegation = _fixture.Create<CreateDelegationRequest>();
        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(true);

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);

        var act = async () => await service.CreateDelegationAsync(contactId, createDelegation);

        var result = Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal("ACC023", result.Result.Code);
        Assert.Equal("Un client ne peut pas émettre ou recevoir de délégation", result.Result.Message);
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenRolesCreated_ShouldPublishHistoryWithAddkdelm()
    {
        var contactId = 25;
        var delegateeId = 42;
        var accountId = 101;
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.DelegateeId, delegateeId)
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .With(p => p.IsRoleToCreate, true)
            .With(p => p.IsAutomaticDelegation, false)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .With(p => p.AccountIds, new List<int> { accountId })
            .With(p => p.IsFullDelegation, false)
            .Create();
        var rolesCreated = new List<CreateRoleRequest>
        {
            new () { AccountId = accountId, ContactId = delegateeId },
        };

        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);
        _repository.Setup(x => x.CreateDelegationAsync(contactId, createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
            .ReturnsAsync(rolesCreated);

        var service = new DelegationService(_repository.Object, _publisher.Object, _historyPublisher.Object, _logger.Object, _roleRepository.Object);
        await service.CreateDelegationAsync(contactId, createDelegation);

        _historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(contactId, delegateeId, accountId, ActionCode.ADDKDELM.ToString()),
            Times.Once);
    }

    [Fact]
    public async Task CreateDelegationAsync_WithMultipleAccounts_ShouldPublishOneHistoryEventPerRole()
    {
        var contactId = 25;
        var delegateeId = 42;
        var accountIds = new List<int> { 101, 102 };
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.DelegateeId, delegateeId)
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .With(p => p.IsRoleToCreate, true)
            .With(p => p.IsAutomaticDelegation, false)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .With(p => p.AccountIds, accountIds)
            .With(p => p.IsFullDelegation, false)
            .Create();
        var rolesCreated = accountIds.Select(id => new CreateRoleRequest { AccountId = id, ContactId = delegateeId }).ToList();

        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);
        _repository.Setup(x => x.CreateDelegationAsync(contactId, createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
            .ReturnsAsync(rolesCreated);

        var service = new DelegationService(_repository.Object, _publisher.Object, _historyPublisher.Object, _logger.Object, _roleRepository.Object);
        await service.CreateDelegationAsync(contactId, createDelegation);

        _historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(contactId, delegateeId, It.IsAny<int>(), ActionCode.ADDKDELM.ToString()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenIsAutomaticDelegation_ShouldStillPublishHistoryWithAddkdelm()
    {
        var contactId = 25;
        var delegateeId = 42;
        var accountId = 101;
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.DelegateeId, delegateeId)
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .With(p => p.IsRoleToCreate, true)
            .With(p => p.IsAutomaticDelegation, true)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .With(p => p.AccountIds, new List<int> { accountId })
            .With(p => p.IsFullDelegation, false)
            .Create();
        var rolesCreated = new List<CreateRoleRequest>
        {
            new () { AccountId = accountId, ContactId = delegateeId },
        };

        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);
        _repository.Setup(x => x.CreateDelegationAsync(contactId, createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
            .ReturnsAsync(rolesCreated);

        var service = new DelegationService(_repository.Object, _publisher.Object, _historyPublisher.Object, _logger.Object, _roleRepository.Object);
        await service.CreateDelegationAsync(contactId, createDelegation);

        _historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(contactId, delegateeId, accountId, ActionCode.ADDKDELM.ToString()),
            Times.Once);
        _historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), ActionCode.ADDKDELA.ToString()),
            Times.Never);
    }

    [Fact]
    public async Task CreateDelegationAsync_WhenNoRoleCreated_ShouldNotPublishHistory()
    {
        var contactId = 25;
        var details = _fixture.Build<DelegationDetails>()
            .With(p => p.StartDate, DateTime.UtcNow)
            .With(p => p.EndDate, DateTime.UtcNow.AddDays(1))
            .With(p => p.IsRoleToCreate, true)
            .CreateMany(1);
        var createDelegation = _fixture.Build<CreateDelegationRequest>()
            .With(p => p.DelegationDetails, details)
            .With(p => p.AccountIds, new List<int> { 101 })
            .With(p => p.IsFullDelegation, false)
            .Create();

        _repository.Setup(x => x.IsClient(It.IsAny<IEnumerable<int>>())).ReturnsAsync(false);
        _repository.Setup(x => x.CreateDelegationAsync(contactId, createDelegation, It.IsAny<IEnumerable<CreateRoleRequest>>()))
            .ReturnsAsync(new List<CreateRoleRequest>());

        var service = new DelegationService(_repository.Object, _publisher.Object, _historyPublisher.Object, _logger.Object, _roleRepository.Object);
        await service.CreateDelegationAsync(contactId, createDelegation);

        _historyPublisher.Verify(
            x => x.PublishHistoryCreatedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
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

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);
        var contactDelegations = await service.GetContactDelegationsAsync(contactId);

        contactDelegations.Should().NotBeNull();
        contactDelegations.Should().BeEquivalentTo(delegations);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_WhenDelegatorExists_ShouldReturnDelegations()
    {
        var delegatorId = 100;
        var filter = new DelegationFilter();
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var expected = _fixture.Create<Paging<Delegation>>();

        _repository.Setup(x => x.DoesContactExistAsync(delegatorId)).ReturnsAsync(true);
        _repository.Setup(x => x.GetDelegatorDelegationsAsync(delegatorId, filter, pagination))
            .ReturnsAsync(expected)
            .Verifiable();

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);
        var result = await service.GetDelegatorDelegationsAsync(delegatorId, filter, pagination);

        result.Should().BeEquivalentTo(expected);
        _repository.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetDelegatorDelegationsAsync_WhenIsAutomaticIsProvided_ShouldPassFilterToRepository(bool isAutomatic)
    {
        var delegatorId = 100;
        var filter = new DelegationFilter { IsAutomatic = isAutomatic };
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var expected = _fixture.Create<Paging<Delegation>>();

        _repository.Setup(x => x.DoesContactExistAsync(delegatorId)).ReturnsAsync(true);
        _repository.Setup(x => x.GetDelegatorDelegationsAsync(delegatorId, filter, pagination))
            .ReturnsAsync(expected)
            .Verifiable();

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);
        var result = await service.GetDelegatorDelegationsAsync(delegatorId, filter, pagination);

        result.Should().BeEquivalentTo(expected);
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_WhenDelegatorDoesNotExist_ShouldThrowNotFoundException()
    {
        var delegatorId = 999;
        var filter = new DelegationFilter();

        _repository.Setup(x => x.DoesContactExistAsync(delegatorId)).ReturnsAsync(false);

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);

        var act = async () => await service.GetDelegatorDelegationsAsync(delegatorId, filter, null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        exception.Code.Should().Be(Errors.NotFoundContactCode);
    }

    [Fact]
    public async Task GetDelegatorDelegationsAsync_WhenPaginationIsNull_ShouldUseDefaults()
    {
        var delegatorId = 100;
        var filter = new DelegationFilter();
        var expected = _fixture.Create<Paging<Delegation>>();

        _repository.Setup(x => x.DoesContactExistAsync(delegatorId)).ReturnsAsync(true);
        _repository.Setup(x => x.GetDelegatorDelegationsAsync(
                delegatorId,
                filter,
                It.Is<Pagination>(p => p.PageNumber == 1 && p.PageSize == int.MaxValue)))
            .ReturnsAsync(expected)
            .Verifiable();

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);
        var result = await service.GetDelegatorDelegationsAsync(delegatorId, filter, null);

        result.Should().BeEquivalentTo(expected);
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

        var service = new DelegationService(_repository.Object, null!, null!, null!, null!);
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

        var service = new DelegationService(_repository.Object, _publisher.Object, _historyPublisher.Object, _logger.Object, _roleRepository.Object);
        await service.DeleteDelegationAsync(1);

        _repository.Verify(x => x.DeleteDelegationAsync(It.IsAny<int>()), Times.Once);
        _publisher.Verify(x => x.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    public async Task DeleteDelegationAsync_WhenDelegationIdIsNegativeOrNull_ShouldThrowBadRequestException(int delegationId)
    {
        var service = new DelegationService(null!, _publisher.Object, null!, null!, null!);

        var result = await Assert.ThrowsAsync<BadRequestException>(async () => await service.DeleteDelegationAsync(delegationId));

        Assert.Equal(Errors.BadRequestDeleteDelegationCode, result.Code);
        Assert.Equal(Errors.BadRequestDeleteDelegationMessage, result.Message);

        _publisher.Verify(p => p.PublishRoleDeletedEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void CreateRoleRequests_ShouldCreateRoleRequestList()
    {
        var contactId = 25;
        var details = _fixture.Build<DelegationDetails>()
            .With(x => x.IsRoleToCreate, true)
            .CreateMany(1);
        var accounts = new List<int> { 1 };
        var expected = _fixture.Build<CreateDelegationRequest>()
            .With(x => x.DelegationDetails, details)
            .With(x => x.AccountIds, accounts)
            .Create();

        var result = DelegationService.CreateRoleRequests(contactId, expected);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var resultItem = result.First();
        resultItem.AccountId.Should().Be(expected.AccountIds!.First());
        resultItem.ContactId.Should().Be(expected.DelegationDetails.First().DelegateeId);
        resultItem.IsFavorite.Should().BeFalse();
        resultItem.IsSignatory.Should().BeFalse();
        resultItem.IsDelegation.Should().BeTrue();
        resultItem.ActionLevel.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(CreateRoleRequestData))]
    public void CreateRoleRequestsWithNullOrEmtpyDelegationDetail_ShouldReturnEmptyList(CreateDelegationRequest delegation)
    {
        var contactId = 25;
        var result = DelegationService.CreateRoleRequests(contactId, delegation);

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
                    DelegationDetails = Enumerable.Empty<DelegationDetails>(),
                    AccountIds = null!,
                    IsFullDelegation = true
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegationDetails = new List<DelegationDetails> { new DelegationDetails { DelegateeId = 1 } },
                    AccountIds = null!,
                    IsFullDelegation = false
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
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
                    DelegationDetails = null!,
                    AccountIds = null!,
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegationDetails = Enumerable.Empty<DelegationDetails>(),
                    AccountIds = null!,
                }
            },
        };
}
