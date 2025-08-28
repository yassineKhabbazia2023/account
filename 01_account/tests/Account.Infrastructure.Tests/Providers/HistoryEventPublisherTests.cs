// <copyright file="HistoryEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class HistoryEventPublisherTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IContactEventRepository> _contactEventRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IEventPublisher> _eventPublisher;

    public HistoryEventPublisherTests()
    {
        _contactEventRepository = new Mock<IContactEventRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _eventPublisher = new Mock<IEventPublisher>();

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_ShouldPublishEvent()
    {
        _contactEventRepository.Setup(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<AccountDetail>());

        var publisher = new HistoryEventPublisher(_contactEventRepository.Object, _accountRepository.Object, _eventPublisher.Object);

        await publisher.PublishHistoryCreatedEventAsync(1, 2, 1, "code");

        _contactEventRepository.Verify(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountAsync(It.IsAny<int>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishHistoryCreatedEventRegistry_ShouldPublishEvent()
    {
        _contactEventRepository.Setup(x => x.GetContactByEmailAsync(It.IsAny<string>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _contactEventRepository.Setup(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<AccountDetail>());

        var publisher = new HistoryEventPublisher(_contactEventRepository.Object, _accountRepository.Object, _eventPublisher.Object);

        await publisher.PublishHistoryCreatedEventAsync("approver@rydge.fr", 2, 1);

        _contactEventRepository.Verify(x => x.GetContactByEmailAsync(It.IsAny<string>()), Times.Once);
        _contactEventRepository.Verify(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Once);
        _accountRepository.Verify(x => x.GetAccountAsync(It.IsAny<int>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Once);
    }
}
