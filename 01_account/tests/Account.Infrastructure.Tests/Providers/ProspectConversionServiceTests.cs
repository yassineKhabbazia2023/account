// <copyright file="ProspectConversionServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Providers;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ProspectConversionServiceTests
{
    private readonly Mock<ILogger<ProspectConversionService>> _loggerMock = new();
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock = new();
    private readonly Mock<IRegistryAccountEventRepository> _accountEventRepositoryMock = new();
    private readonly Mock<IAccountEventPublisher> _accountEventPublisherMock = new();

    private ProspectConversionService CreateService() => new(
        _loggerMock.Object,
        _featureFlagServiceMock.Object,
        _accountEventRepositoryMock.Object,
        _accountEventPublisherMock.Object);

    private void EnableFlag(bool enabled) =>
        _featureFlagServiceMock
            .Setup(f => f.IsEnabledAsync(FeatureFlagKeys.ProspectToClientConversion, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enabled);

    private static RegistryAccountCreatedEventData ClientEvent(string? siret = "12345678901234") =>
        new()
        {
            AccountGlobalUniqueIdentifier = Guid.NewGuid(),
            AccountType = AccountType.CLIENT.ToString(),
            AccountRegisterIdentification1 = siret,
        };

    [Fact]
    public async Task HandleClientCreatedAsync_WhenFlagDisabled_ShouldDoNothing()
    {
        EnableFlag(false);

        await CreateService().HandleClientCreatedAsync(ClientEvent());

        _accountEventRepositoryMock.Verify(
            r => r.FindActiveProspectsBySiretAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleClientCreatedAsync_WhenNotClient_ShouldDoNothing()
    {
        EnableFlag(true);
        var prospectEvent = ClientEvent();
        prospectEvent.AccountType = GlobalConstants.ProspectAccountType;

        await CreateService().HandleClientCreatedAsync(prospectEvent);

        _accountEventRepositoryMock.Verify(
            r => r.FindActiveProspectsBySiretAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleClientCreatedAsync_WhenSiretEmpty_ShouldDoNothing(string? siret)
    {
        EnableFlag(true);

        await CreateService().HandleClientCreatedAsync(ClientEvent(siret));

        _accountEventRepositoryMock.Verify(
            r => r.FindActiveProspectsBySiretAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleClientCreatedAsync_WhenNoProspectFound_ShouldNotRemoveAnything()
    {
        EnableFlag(true);
        _accountEventRepositoryMock
            .Setup(r => r.FindActiveProspectsBySiretAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<ProspectRef>());

        await CreateService().HandleClientCreatedAsync(ClientEvent());

        _accountEventRepositoryMock.Verify(r => r.RemoveAccountAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleClientCreatedAsync_WithSingleProspect_ShouldDeactivateAccountAndLeaveRoles()
    {
        EnableFlag(true);
        var prospect = new ProspectRef(42, Guid.NewGuid());
        _accountEventRepositoryMock
            .Setup(r => r.FindActiveProspectsBySiretAsync(It.IsAny<string>()))
            .ReturnsAsync(new[] { prospect });
        _accountEventRepositoryMock
            .Setup(r => r.RemoveAccountAsync(prospect.AccountId))
            .ReturnsAsync((prospect.AccountId, GlobalConstants.ProspectAccountType));

        await CreateService().HandleClientCreatedAsync(ClientEvent());

        _accountEventRepositoryMock.Verify(r => r.RemoveAccountAsync(prospect.AccountId), Times.Once);
        _accountEventPublisherMock.Verify(
            p => p.PublishAccountRemovedEventAsync(prospect.AccountId, GlobalConstants.ProspectAccountType),
            Times.Once);
    }

    [Fact]
    public async Task HandleClientCreatedAsync_WithSeveralProspects_ShouldAbortAndNotRemoveAnything()
    {
        EnableFlag(true);
        _accountEventRepositoryMock
            .Setup(r => r.FindActiveProspectsBySiretAsync(It.IsAny<string>()))
            .ReturnsAsync(new[] { new ProspectRef(1, Guid.NewGuid()), new ProspectRef(2, Guid.NewGuid()) });

        await CreateService().HandleClientCreatedAsync(ClientEvent());

        _accountEventRepositoryMock.Verify(r => r.RemoveAccountAsync(It.IsAny<int>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}
