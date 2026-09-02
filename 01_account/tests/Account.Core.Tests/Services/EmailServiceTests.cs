// <copyright file="EmailServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using Notifications.Commons.WebApi.QueryParams;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Options;
using Pulse.Account.Core.Services;
using Pulse.Back.Events.Abstractions;

namespace Pulse.Account.Core.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<INotificationManager> _mockNotificationManager;
    private readonly Mock<IContactRepository> _mockContactRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly EmailOptions _options;
    private readonly EmailService _service;

    public EmailServiceTests()
    {
        _mockNotificationManager = new Mock<INotificationManager>();
        _mockContactRepository = new Mock<IContactRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _options = new EmailOptions
        {
            DelegationRequestEmailTemplate = "delegation-request-template",
            SenderEmail = "noreply@pulse.test",
            ServiceBusTopic = "email-topic",
            WalletBaseUrl = "https://wallet.pulse.test"
        };

        _mockContactRepository
            .Setup(r => r.GetContactByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Contact
            {
                ContactId = id,
                FirstName = $"first{id}",
                LastName = $"last{id}",
                Email = $"contact{id}@pulse.test"
            });

        _mockAccountRepository
            .Setup(r => r.GetAccountAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new AccountDetail
            {
                AccountId = id,
                AccountNumber = $"ACC{id}",
                Legal = new Legal { LegalName = $"Legal{id}" },
                Phone = new List<Phone>()
            });

        _service = new EmailService(
            _mockNotificationManager.Object,
            Microsoft.Extensions.Options.Options.Create(_options),
            _mockContactRepository.Object,
            _mockAccountRepository.Object);
    }

    [Fact]
    public async Task SendDelegationRequestEmailsAsync_ShouldPublishEmailEventPerRecipientOnConfiguredTopic()
    {
        await _service.SendDelegationRequestEmailsAsync(new[] { 2, 3 }, 1, 100);

        _mockNotificationManager.Verify(
            n => n.PublishAsync(It.IsAny<BaseEvent<EmailRequest>>(), _options.ServiceBusTopic),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SendDelegationRequestEmailsAsync_ShouldBuildEmailRequestFromContactAndAccount()
    {
        var publishedRequests = new List<EmailRequest>();
        _mockNotificationManager
            .Setup(n => n.PublishAsync(It.IsAny<BaseEvent<EmailRequest>>(), It.IsAny<string>()))
            .Callback<BaseEvent<EmailRequest>, string>((emailEvent, _) => publishedRequests.Add(emailEvent.Data))
            .Returns(Task.CompletedTask);

        await _service.SendDelegationRequestEmailsAsync(new[] { 2 }, 1, 100);

        publishedRequests.Should().ContainSingle();
        var publishedRequest = publishedRequests[0];
        publishedRequest.From.Should().Be(_options.SenderEmail);
        publishedRequest.To.Should().ContainSingle().Which.Should().Be("contact2@pulse.test");
        publishedRequest.TemplateName.Should().Be(_options.DelegationRequestEmailTemplate);

        var variables = publishedRequest.Variables;
        variables["userName"].Should().Be("First2 Last2");
        variables["requestorName"].Should().Be("First1 Last1");
        variables["requestorEmail"].Should().Be("contact1@pulse.test");
        variables["legalName"].Should().Be("Legal100");
        variables["accountNumber"].Should().Be("ACC100");
        variables["delegationRequestUrl"].Should().Be($"{_options.WalletBaseUrl}/redirect?action=open-delegation-drawer");
    }

    [Fact]
    public async Task SendDelegationRequestEmailsAsync_WhenNoRecipient_ShouldNotPublish()
    {
        await _service.SendDelegationRequestEmailsAsync(new List<int>(), 1, 100);

        _mockNotificationManager.Verify(
            n => n.PublishAsync(It.IsAny<BaseEvent<EmailRequest>>(), It.IsAny<string>()),
            Times.Never);
    }
}
