// <copyright file="OfferEligibilityServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Services;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Services;

public class OfferEligibilityServiceTests
{
    private readonly Mock<IOfferEligibilityRepository> _offerEligibilityRepositoryMock;
    private readonly Mock<IContactRepository> _contactRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IReportEventPublisher> _reportEventPublisherMock;
    private readonly Mock<IOfferActivatedEventPublisher> _publisher;
    private readonly OfferEligibilityService _service;

    public OfferEligibilityServiceTests()
    {
        _offerEligibilityRepositoryMock = new Mock<IOfferEligibilityRepository>();
        _contactRepositoryMock = new Mock<IContactRepository>();
        _reportEventPublisherMock = new Mock<IReportEventPublisher>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _publisher = new Mock<IOfferActivatedEventPublisher>();
        _service = new OfferEligibilityService(_offerEligibilityRepositoryMock.Object, _contactRepositoryMock.Object, _roleRepositoryMock.Object, _publisher.Object, _reportEventPublisherMock.Object);
    }

    [Fact]
    public async Task GetOfferEligibilityByIdAsync_Should_ReturnEntity()
    {
        // Arrange
        var accountId = 123;
        var expected = new OfferEligibility { AccountId = accountId, IsEligible = true };
        _offerEligibilityRepositoryMock
            .Setup(r => r.GetOfferEligibilityByIdAsync(accountId))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetOfferEligibilityByIdAsync(accountId);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateOfferEligibilityAsync_Should_ThrowBadRequest_WhenContactIsCollab()
    {
        // Arrange
        var accountId = 456;
        var contact = new Contact
        {
            ContactId = 1,
            Type = ContactType.Collaborator.ToString(),
            Email = "collab@test.com",
            FirstName = "Test",
            LastName = "Test",
        };
        _contactRepositoryMock
            .Setup(c => c.GetContactByIdAsync(contact.ContactId))
            .ReturnsAsync(contact);

        // Act
        Func<Task> act = async () => await _service.UpdateOfferEligibilityAsync(contact.ContactId, accountId);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(Errors.NotPermittedActionMessage);
    }

    [Fact]
    public async Task UpdateOfferEligibilityAsync_Should_ThrowForbiddenException_WhenContactHasNoRoleOnAccount()
    {
        // Arrange
        var accountId = 456;
        var contact = new Contact
        {
            ContactId = 1,
            Type = ContactType.Customer.ToString(),
            Email = "collab@test.com",
            FirstName = "Test",
            LastName = "Test",
        };
        _contactRepositoryMock.Setup(c => c.GetContactByIdAsync(contact.ContactId)).ReturnsAsync(contact);
        _roleRepositoryMock.Setup(x => x.IsContactHasRoleOnAccount(contact.ContactId, accountId, null)).ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _service.UpdateOfferEligibilityAsync(contact.ContactId, accountId);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Le contact avec l'identifiant 1 n'a aucun role sur l'account 456");
    }

    [Fact]
    public async Task UpdateOfferEligibilityAsync_Should_ThrowBadRequest_WhenOfferAlreadyActive()
    {
        // Arrange
        var accountId = 789;
        var contact = new Contact
        {
            ContactId = 2,
            Type = "Customer",
            Email = "admin@test.com",
            FirstName = "Test",
            LastName = "Test",
        };

        _contactRepositoryMock
            .Setup(c => c.GetContactByIdAsync(contact.ContactId))
            .ReturnsAsync(contact);

        _roleRepositoryMock.Setup(x => x.IsContactHasRoleOnAccount(contact.ContactId, accountId, null)).ReturnsAsync(true);

        _offerEligibilityRepositoryMock
            .Setup(r => r.IsOfferEligibilityActiveAsync(accountId))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.UpdateOfferEligibilityAsync(contact.ContactId, accountId);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(Errors.AlreadyActiveOfferEligibilityMessage);
    }

    [Fact]
    public async Task UpdateOfferEligibilityAsync_Should_UpdateAndReturnOfferEligibility()
    {
        // Arrange
        var accountId = 321;
        var contact = new Contact
        {
            ContactId = 3,
            Type = "Customer",
            Email = "admin@test.com",
            FirstName = "Test",
            LastName = "Test",
        };
        var expected = new OfferEligibility
        {
            AccountId = accountId,
            IsEligible = true,
            Reporting = new Reporting
            {
                ReportId = 3,
                ReportLabel = "Test",
            }
        };

        _contactRepositoryMock
            .Setup(c => c.GetContactByIdAsync(contact.ContactId))
            .ReturnsAsync(contact);

        _roleRepositoryMock.Setup(x => x.IsContactHasRoleOnAccount(contact.ContactId, accountId, null)).ReturnsAsync(true);

        _offerEligibilityRepositoryMock
            .Setup(r => r.IsOfferEligibilityActiveAsync(accountId))
            .ReturnsAsync(false);

        _offerEligibilityRepositoryMock
            .Setup(r => r.UpdateOfferEligibilityAsync(accountId, contact.Email))
            .ReturnsAsync(expected);

        _reportEventPublisherMock
            .Setup(r => r.PublishReportCreatedEventAsync(expected.Reporting.ReportId, accountId, GlobalConstants.CLARITYREPORTTYPEID, expected.Reporting.ReportLabel, ReportStatus.ONLINE))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateOfferEligibilityAsync(contact.ContactId, accountId);

        // Assert
        result.Should().BeEquivalentTo(expected);
        _offerEligibilityRepositoryMock.Verify(r => r.UpdateOfferEligibilityAsync(accountId, contact.Email), Times.Once);
        _publisher.Verify(p => p.PublishOfferActivatedEventAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }
}
