// <copyright file="ReportCreatedEventHandlerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ReportCreatedEventHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<ReportCreatedEventHandler>> _logger;
    private readonly Mock<IAccountEventRepository> _accountEventRepository;
    private readonly Mock<IOfferEligibilityEventRepository> _offerEligibilityRepository;

    public ReportCreatedEventHandlerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _logger = new Mock<ILogger<ReportCreatedEventHandler>>();
        _accountEventRepository = new Mock<IAccountEventRepository>();
        _offerEligibilityRepository = new Mock<IOfferEligibilityEventRepository>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_ShouldNotCreateOfferEligibilityEntity_WhenMessageIsNullOrEmpty(string message)
    {
        var handler = new ReportCreatedEventHandler(_logger.Object, _accountEventRepository.Object, _offerEligibilityRepository.Object);

        await handler.HandleAsync(message);

        _accountEventRepository.Verify(x => x.DoesAccountExistAsync(It.IsAny<int>()), Times.Never);
        _offerEligibilityRepository.Verify(x => x.DoesOfferEligibilityExistsAsync(It.IsAny<int>()), Times.Never);
        _offerEligibilityRepository.Verify(x => x.CreateOfferEligibilityAsync(It.IsAny<OfferEligibilityEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCreateOfferEligibilityEntity_When_ReportEventIsInvalid()
    {
        var evt = new ReportCreatedEvent(new ReportCreatedEventData { ReportId = 0, AccountId = -2 });

        var json = JsonConvert.SerializeObject(evt);
        var handler = new ReportCreatedEventHandler(_logger.Object, _accountEventRepository.Object, _offerEligibilityRepository.Object);

        await handler.HandleAsync(json);

        _accountEventRepository.Verify(x => x.DoesAccountExistAsync(It.IsAny<int>()), Times.Never);
        _offerEligibilityRepository.Verify(x => x.DoesOfferEligibilityExistsAsync(It.IsAny<int>()), Times.Never);
        _offerEligibilityRepository.Verify(x => x.CreateOfferEligibilityAsync(It.IsAny<OfferEligibilityEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotCreateOfferEligibilityEntity_WhenAccountDoesNotExist()
    {
        var evt = new ReportCreatedEvent(new ReportCreatedEventData { ReportId = 1, AccountId = 2, ReportTypeId = 9 });
        var json = JsonConvert.SerializeObject(evt);

        _accountEventRepository.Setup(x => x.DoesAccountExistAsync(2)).ReturnsAsync(false);

        var handler = new ReportCreatedEventHandler(_logger.Object, _accountEventRepository.Object, _offerEligibilityRepository.Object);
        await handler.HandleAsync(json);

        _accountEventRepository.Verify(x => x.DoesAccountExistAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.DoesOfferEligibilityExistsAsync(It.IsAny<int>()), Times.Never);
        _offerEligibilityRepository.Verify(x => x.CreateOfferEligibilityAsync(It.IsAny<OfferEligibilityEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_NotCreateOfferEligibilityEntity_WhenOfferEligibilityExists()
    {
        var evt = new ReportCreatedEvent(new ReportCreatedEventData { ReportId = 1, AccountId = 2, ReportTypeId = 9 });
        var json = JsonConvert.SerializeObject(evt);

        _accountEventRepository.Setup(x => x.DoesAccountExistAsync(2)).ReturnsAsync(true);
        _offerEligibilityRepository.Setup(x => x.DoesOfferEligibilityExistsAsync(2)).ReturnsAsync(true);

        var handler = new ReportCreatedEventHandler(_logger.Object, _accountEventRepository.Object, _offerEligibilityRepository.Object);
        await handler.HandleAsync(json);

        _accountEventRepository.Verify(x => x.DoesAccountExistAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.DoesOfferEligibilityExistsAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.CreateOfferEligibilityAsync(It.IsAny<OfferEligibilityEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_NotCreateOfferEligibilityEntity_WhenReportIsNotClarityType()
    {
        var evt = new ReportCreatedEvent(new ReportCreatedEventData { ReportId = 1, AccountId = 2, ReportTypeId = 8 });
        var json = JsonConvert.SerializeObject(evt);

        _accountEventRepository.Setup(x => x.DoesAccountExistAsync(2)).ReturnsAsync(true);
        _offerEligibilityRepository.Setup(x => x.DoesOfferEligibilityExistsAsync(2)).ReturnsAsync(false);

        var handler = new ReportCreatedEventHandler(_logger.Object, _accountEventRepository.Object, _offerEligibilityRepository.Object);
        await handler.HandleAsync(json);

        _accountEventRepository.Verify(x => x.DoesAccountExistAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.DoesOfferEligibilityExistsAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.CreateOfferEligibilityAsync(It.IsAny<OfferEligibilityEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Nominal()
    {
        var evt = new ReportCreatedEvent(new ReportCreatedEventData { ReportId = 1, AccountId = 2, ReportTypeId = 9 });
        var json = JsonConvert.SerializeObject(evt);

        _accountEventRepository.Setup(x => x.DoesAccountExistAsync(2)).ReturnsAsync(true);
        _offerEligibilityRepository.Setup(x => x.DoesOfferEligibilityExistsAsync(2)).ReturnsAsync(false);

        var handler = new ReportCreatedEventHandler(_logger.Object, _accountEventRepository.Object, _offerEligibilityRepository.Object);
        await handler.HandleAsync(json);

        _accountEventRepository.Verify(x => x.DoesAccountExistAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.DoesOfferEligibilityExistsAsync(It.IsAny<int>()), Times.Once);
        _offerEligibilityRepository.Verify(x => x.CreateOfferEligibilityAsync(It.Is<OfferEligibilityEntity>(o =>
        o.AccountId == 2 &&
        o.OfferName == "Clarity" &&
        o.IsEligible == true)), Times.Once);
    }
}
