// <copyright file="HistoryEventPublisherTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class HistoryEventPublisherTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IContactRepository> _contactRepository;
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<IEventPublisher> _eventPublisher;

    public HistoryEventPublisherTests()
    {
        _contactRepository = new Mock<IContactRepository>();
        _accountRepository = new Mock<IAccountRepository>();
        _eventPublisher = new Mock<IEventPublisher>();

        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_ShouldPublishEvent()
    {
        _contactRepository.Setup(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(_fixture.Create<AccountDetail>());

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _eventPublisher.Object);

        await publisher.PublishHistoryCreatedEventAsync(1, 2, 1);

        _contactRepository.Verify(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountAsync(It.IsAny<int>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Once);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoCurrentUserFound_ShouldThrowNotFoundException()
    {
        ContactEntity invalidContact = null!;
        _contactRepository.Setup(x => x.GetContactAsync(1, false)).ReturnsAsync(invalidContact);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEventAsync(1, 2, 1));

        Assert.Equal("ACC002", result.Code);
        Assert.Equal("Le contact avec l'identifiant 1 est introuvable", result.Message);

        _contactRepository.Verify(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Once);
        _accountRepository.Verify(x => x.GetAccountAsync(It.IsAny<int>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoTargetUserFound_ShouldThrowNotFoundException()
    {
        ContactEntity invalidContact = null!;
        _contactRepository.Setup(x => x.GetContactAsync(1, false)).ReturnsAsync(_fixture.Create<ContactEntity>());
        _contactRepository.Setup(x => x.GetContactAsync(2, false)).ReturnsAsync(invalidContact);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEventAsync(1, 2, 1));

        Assert.Equal("ACC002", result.Code);
        Assert.Equal("Le contact avec l'identifiant 2 est introuvable", result.Message);

        _contactRepository.Verify(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountAsync(It.IsAny<int>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }

    [Fact]
    public async Task PublishHistoryCreatedEvent_WithNoAccountFound_ShouldThrowNotFoundException()
    {
        AccountDetail invalidAccount = null!;
        _contactRepository.Setup(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>())).ReturnsAsync(_fixture.Create<ContactEntity>());
        _accountRepository.Setup(x => x.GetAccountAsync(It.IsAny<int>())).ReturnsAsync(invalidAccount);

        var publisher = new HistoryEventPublisher(_contactRepository.Object, _accountRepository.Object, _eventPublisher.Object);

        var result = await Assert.ThrowsAsync<NotFoundException>(async () => await publisher.PublishHistoryCreatedEventAsync(1, 2, 1));

        Assert.Equal("ACC001", result.Code);
        Assert.Equal("L'entité avec l'identifiant 1 est introuvable", result.Message);

        _contactRepository.Verify(x => x.GetContactAsync(It.IsAny<int>(), It.IsAny<bool?>()), Times.Exactly(2));
        _accountRepository.Verify(x => x.GetAccountAsync(It.IsAny<int>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<BaseEvent<HistoryCreatedEventData>>(), null!, null), Times.Never);
    }
}
