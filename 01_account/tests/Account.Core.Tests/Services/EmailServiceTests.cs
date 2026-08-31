// <copyright file="EmailServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using Notifications.Commons.WebApi.QueryParams;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models.Email;
using Pulse.Account.Core.Options;
using Pulse.Account.Core.Services;
using Pulse.Back.Events.Abstractions;

namespace Pulse.Account.Core.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<INotificationManager> _mockNotificationManager;
    private readonly EmailOptions _options;
    private readonly EmailService _service;

    public EmailServiceTests()
    {
        _mockNotificationManager = new Mock<INotificationManager>();
        _options = new EmailOptions
        {
            DelegationRequestEmailTemplate = "delegation-request-template",
            SenderEmail = "noreply@pulse.test",
            ServiceBusTopic = "email-topic",
            WalletBaseUrl = "https://wallet.pulse.test"
        };

        _service = new EmailService(_mockNotificationManager.Object, Microsoft.Extensions.Options.Options.Create(_options));
    }

    [Fact]
    public async Task SendRequestEmailAsync_ShouldPublishEmailEventOnConfiguredTopic()
    {
        var context = CreateContext();

        await _service.SendRequestEmailAsync(context);

        _mockNotificationManager.Verify(
            n => n.PublishAsync(It.IsAny<BaseEvent<EmailRequest>>(), _options.ServiceBusTopic),
            Times.Once);
    }

    [Fact]
    public async Task SendRequestEmailAsync_ShouldBuildEmailRequestFromContextAndOptions()
    {
        var context = CreateContext();
        EmailRequest? publishedRequest = null;

        _mockNotificationManager
            .Setup(n => n.PublishAsync(It.IsAny<BaseEvent<EmailRequest>>(), It.IsAny<string>()))
            .Callback<BaseEvent<EmailRequest>, string>((emailEvent, _) => publishedRequest = emailEvent.Data)
            .Returns(Task.CompletedTask);

        await _service.SendRequestEmailAsync(context);

        publishedRequest.Should().NotBeNull();
        publishedRequest!.From.Should().Be(_options.SenderEmail);
        publishedRequest.To.Should().ContainSingle().Which.Should().Be(context.RecipientEmail);
        publishedRequest.TemplateName.Should().Be(_options.DelegationRequestEmailTemplate);
    }

    [Fact]
    public async Task SendRequestEmailAsync_ShouldPopulateVariablesWithCapitalizedNamesAndFormattedDate()
    {
        var context = CreateContext();
        EmailRequest? publishedRequest = null;

        _mockNotificationManager
            .Setup(n => n.PublishAsync(It.IsAny<BaseEvent<EmailRequest>>(), It.IsAny<string>()))
            .Callback<BaseEvent<EmailRequest>, string>((emailEvent, _) => publishedRequest = emailEvent.Data)
            .Returns(Task.CompletedTask);

        await _service.SendRequestEmailAsync(context);

        publishedRequest.Should().NotBeNull();
        var variables = publishedRequest!.Variables;
        variables["userName"].Should().Be("John Doe");
        variables["requestorName"].Should().Be("Jane Smith");
        variables["requestorEmail"].Should().Be(context.RequestorEmail);
        variables["legalName"].Should().Be(context.LegalName);
        variables["accountNumber"].Should().Be(context.AccountNumber);
        variables["date"].Should().Be("15/01/2024");
        variables["time"].Should().Be("09:30");
        variables["delegationRequestUrl"].Should().Be($"{_options.WalletBaseUrl}/redirect?action=open-delegation-drawer");
    }

    private static RequestEmailContext CreateContext()
    {
        return new RequestEmailContext
        {
            RecipientEmail = "recipient@pulse.test",
            UserFirstName = "john",
            UserLastName = "doe",
            RequestorFirstName = "jane",
            RequestorLastName = "smith",
            RequestorEmail = "jane.smith@pulse.test",
            LegalName = "Legal Corp",
            AccountNumber = "ACC-123",
            Date = new DateTime(2024, 1, 15, 9, 30, 0, DateTimeKind.Utc)
        };
    }
}
