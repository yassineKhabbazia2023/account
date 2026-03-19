// <copyright file="ReportEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ReportEventPublisherTests
{
    [Fact]
    public async Task PublishReportCreatedEventAsync_Should_PublishEvent()
    {
        // Arrange
        var publisherMock = new Mock<IEventPublisher>();
        var accountRepositoryMock = new Mock<IAccountRepository>();

        var accountId = 1;
        var reportId = 1;
        var reportTypeId = 1;
        var reportLabel = "label";
        var reportStatus = ReportStatus.ONLINE;

        accountRepositoryMock.Setup(a => a.GetAccountAsync(accountId)).ReturnsAsync(new AccountDetail
        {
            AccountId = accountId,
            AccountNumber = "123",
            AccountType = "CLIENT",
            Legal = new Legal { LegalName = "Test" },
            Phone = new List<Phone>(),
        });

        publisherMock.Setup(p => p.PublishAsync(It.IsAny<BaseEvent<ReportCreatedEventData>>(), null!, null)).Callback<BaseEvent<ReportCreatedEventData>, string, string>((@event, _, _) =>
        {
            Assert.Equal(reportId, @event.Data.ReportId);
            Assert.Equal(accountId, @event.Data.AccountId);
            Assert.Equal(reportLabel, @event.Data.ReportLabel);
            Assert.Equal(reportTypeId, @event.Data.ReportTypeId);
            Assert.Equal(reportStatus.ToString(), @event.Data.ReportStatus);
        }).Returns(Task.CompletedTask).Verifiable();

        var reportEventPublisher = new ReportEventPublisher(publisherMock.Object, accountRepositoryMock.Object);

        // Act
        await reportEventPublisher.PublishReportCreatedEventAsync(reportId, accountId, reportTypeId, reportLabel, reportStatus);

        // Assert
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<BaseEvent<ReportCreatedEventData>>(), null!, null), Times.Once);
    }
}
