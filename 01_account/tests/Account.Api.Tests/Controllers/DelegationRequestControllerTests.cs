// <copyright file="DelegationRequestControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Account.Api.Tests.Controllers;

public class DelegationRequestControllerTests
{
    private readonly Mock<IDelegationService> _delegationService;
    private readonly Mock<IDelegationRequestService> _delegationRequestService;
    private readonly Fixture _fixture;

    public DelegationRequestControllerTests()
    {
        _delegationService = new Mock<IDelegationService>(MockBehavior.Strict);
        _delegationRequestService = new Mock<IDelegationRequestService>(MockBehavior.Strict);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenRequestIsValid_ShouldReturnCreatedResult()
    {
        var contactId = 25;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2, 3 }
        };

        _delegationRequestService.Setup(x => x.CreateDelegationRequestsAsync(contactId, request))
            .ReturnsAsync(new CreateDelegationRequestsResponse { CreatedRecipientIds = new[] { 2, 3 }, Errors = new List<RecipientError>() })
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.CreateDelegationRequestsAsync(contactId, request);

        actionResult.Result.Should().BeOfType<CreatedResult>();
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenServiceThrowsBadRequestException_ShouldPropagate()
    {
        var contactId = 25;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 100,
            RecipientIds = new[] { 2 }
        };

        _delegationRequestService.Setup(x => x.CreateDelegationRequestsAsync(contactId, request))
            .ThrowsAsync(new BadRequestException(Errors.CreateDelegationCode, Errors.CreateDelegationMessage));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        var result = await Assert.ThrowsAsync<BadRequestException>(async () => await controller.CreateDelegationRequestsAsync(contactId, request));

        result.Code.Should().Be(Errors.CreateDelegationCode);
        result.Message.Should().Be(Errors.CreateDelegationMessage);
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_WhenServiceThrowsNotFoundException_ShouldPropagate()
    {
        var contactId = 25;
        var request = new CreateDelegationRequestsRequest
        {
            AccountId = 999,
            RecipientIds = new[] { 2 }
        };

        _delegationRequestService.Setup(x => x.CreateDelegationRequestsAsync(contactId, request))
            .ThrowsAsync(new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, request.AccountId)));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await controller.CreateDelegationRequestsAsync(contactId, request));

        result.Code.Should().Be(Errors.NotFoundAccountCode);
    }

    [Fact]
    public async Task GetSentRequestsAsync_WhenCalled_ShouldReturnOkResultWithPaging()
    {
        var contactId = 25;
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var expected = _fixture.Create<Paging<DelegationRequest>>();

        _delegationRequestService.Setup(x => x.GetSentRequestsAsync(contactId, pagination))
            .ReturnsAsync(expected)
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.GetSentRequestsAsync(contactId, pagination);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expected);
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task GetSentRequestsAsync_WhenServiceThrowsBadRequestException_ShouldPropagate()
    {
        var contactId = 25;

        _delegationRequestService.Setup(x => x.GetSentRequestsAsync(contactId, It.IsAny<Pagination>()))
            .ThrowsAsync(new BadRequestException("BAD001", "Bad request"));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        await Assert.ThrowsAsync<BadRequestException>(async () => await controller.GetSentRequestsAsync(contactId, null));
    }

    [Fact]
    public async Task GetReceivedRequestsAsync_WhenCalled_ShouldReturnOkResultWithPaging()
    {
        var contactId = 25;
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
        var expected = _fixture.Create<Paging<DelegationRequest>>();

        _delegationRequestService.Setup(x => x.GetReceivedRequestsAsync(contactId, pagination, null))
            .ReturnsAsync(expected)
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.GetReceivedRequestsAsync(contactId, pagination, null);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expected);
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task GetReceivedRequestsAsync_WhenServiceThrowsBadRequestException_ShouldPropagate()
    {
        var contactId = 25;

        _delegationRequestService.Setup(x => x.GetReceivedRequestsAsync(contactId, It.IsAny<Pagination>(), It.IsAny<DelegationRequestStatus[]>()))
            .ThrowsAsync(new BadRequestException("BAD001", "Bad request"));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        await Assert.ThrowsAsync<BadRequestException>(async () => await controller.GetReceivedRequestsAsync(contactId, null, null));
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenEligible_ShouldReturnOkResultWithEligibilityResponse()
    {
        var contactId = 25;
        var accountId = 100;
        var expected = new DelegationEligibilityResponse
        {
            IsEligible = true,
            Reason = null
        };

        _delegationRequestService.Setup(x => x.CheckEligibilityAsync(contactId, accountId))
            .ReturnsAsync(expected)
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.CheckEligibilityAsync(contactId, accountId);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expected);
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenNotEligible_ShouldReturnOkResultWithEligibilityResponse()
    {
        var contactId = 25;
        var accountId = 100;
        var expected = new DelegationEligibilityResponse
        {
            IsEligible = false,
            Reason = "AlreadyInPortfolio"
        };

        _delegationRequestService.Setup(x => x.CheckEligibilityAsync(contactId, accountId))
            .ReturnsAsync(expected)
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.CheckEligibilityAsync(contactId, accountId);

        actionResult.Result.As<OkObjectResult>().StatusCode.Should().Be(200);
        actionResult.Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expected);
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task CheckEligibilityAsync_WhenServiceThrowsBadRequestException_ShouldPropagate()
    {
        var contactId = 25;
        var accountId = 100;

        _delegationRequestService.Setup(x => x.CheckEligibilityAsync(contactId, accountId))
            .ThrowsAsync(new BadRequestException("BAD001", "Bad request"));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        await Assert.ThrowsAsync<BadRequestException>(async () => await controller.CheckEligibilityAsync(contactId, accountId));
    }

    // ========== AcceptRequests ==========

    [Fact]
    public async Task AcceptRequests_WhenRequestIsValid_ShouldReturnOkResultWithResponse()
    {
        var contactId = 25;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 1, 2 } };
        var expectedResponse = new ProcessDelegationRequestsResponse
        {
            ProcessedIds = new[] { 1, 2 },
            Errors = new List<DelegationRequestError>()
        };

        _delegationRequestService.Setup(x => x.AcceptRequestsAsync(contactId, request))
            .ReturnsAsync(expectedResponse)
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.AcceptRequests(contactId, request);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task AcceptRequests_WhenServiceThrowsBadRequestException_ShouldPropagate()
    {
        var contactId = 25;
        var request = new AcceptDelegationRequestsRequest { DelegationRequestIds = new[] { 999 } };

        _delegationRequestService.Setup(x => x.AcceptRequestsAsync(contactId, request))
            .ThrowsAsync(new BadRequestException(Errors.DelegationRequestNotFoundCode, "Not found"));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        await Assert.ThrowsAsync<BadRequestException>(async () => await controller.AcceptRequests(contactId, request));
    }

    // ========== RefuseRequests ==========

    [Fact]
    public async Task RefuseRequests_WhenRequestIsValid_ShouldReturnOkResultWithResponse()
    {
        var contactId = 25;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 1 } };
        var expectedResponse = new ProcessDelegationRequestsResponse
        {
            ProcessedIds = new[] { 1 },
            Errors = new List<DelegationRequestError>()
        };

        _delegationRequestService.Setup(x => x.RefuseRequestsAsync(contactId, request))
            .ReturnsAsync(expectedResponse)
            .Verifiable();

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);
        var actionResult = await controller.RefuseRequests(contactId, request);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
        _delegationRequestService.VerifyAll();
    }

    [Fact]
    public async Task RefuseRequests_WhenServiceThrowsBadRequestException_ShouldPropagate()
    {
        var contactId = 25;
        var request = new RefuseDelegationRequestsRequest { DelegationRequestIds = new[] { 999 } };

        _delegationRequestService.Setup(x => x.RefuseRequestsAsync(contactId, request))
            .ThrowsAsync(new BadRequestException(Errors.DelegationRequestNotFoundCode, "Not found"));

        var controller = new DelegationController(_delegationService.Object, _delegationRequestService.Object);

        await Assert.ThrowsAsync<BadRequestException>(async () => await controller.RefuseRequests(contactId, request));
    }
}
