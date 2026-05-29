// <copyright file="DelegationRequestMapperTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class DelegationRequestMapperTests
{
    [Fact]
    public void ToDelegationRequest_ShouldMapAllFieldsIncludingNestedObjects()
    {
        // Arrange
        var createdAt = new DateTime(2025, 04, 10, 8, 30, 0, DateTimeKind.Utc);
        var respondedAt = createdAt.AddDays(2);

        var source = new DelegationRequestEntity
        {
            DelegationRequestId = 100,
            RequesterId = 11,
            RecipientId = 22,
            AccountId = 33,
            CreatedAt = createdAt,
            Status = "accepted",
            RespondedAt = respondedAt,
            Requester = new ContactEntity
            {
                ContactId = 11,
                FirstName = "Alice",
                LastName = "Requester",
                Email = "alice@test.fr"
            },
            Recipient = new ContactEntity
            {
                ContactId = 22,
                FirstName = "Bob",
                LastName = "Recipient",
                Email = "bob@test.fr"
            },
            Account = new AccountEntity
            {
                AccountId = 33,
                AccountNumber = "ACC-33",
                LegalName = "Pulse Legal"
            }
        };

        // Act
        var result = source.ToDelegationRequest();

        // Assert
        result.Should().NotBeNull();
        result.DelegationRequestId.Should().Be(source.DelegationRequestId);
        result.RequesterId.Should().Be(source.RequesterId);
        result.RecipientId.Should().Be(source.RecipientId);
        result.AccountId.Should().Be(source.AccountId);
        result.CreatedAt.Should().Be(source.CreatedAt);
        result.Status.Should().Be(source.Status);
        result.RespondedAt.Should().Be(source.RespondedAt);

        result.Requester.Should().NotBeNull();
        result.Requester!.ContactId.Should().Be(source.Requester.ContactId);
        result.Requester.FirstName.Should().Be(source.Requester.FirstName);
        result.Requester.LastName.Should().Be(source.Requester.LastName);
        result.Requester.Email.Should().Be(source.Requester.Email);

        result.Recipient.Should().NotBeNull();
        result.Recipient!.ContactId.Should().Be(source.Recipient.ContactId);
        result.Recipient.FirstName.Should().Be(source.Recipient.FirstName);
        result.Recipient.LastName.Should().Be(source.Recipient.LastName);
        result.Recipient.Email.Should().Be(source.Recipient.Email);

        result.Account.Should().NotBeNull();
        result.Account!.AccountId.Should().Be(source.Account.AccountId);
        result.Account.AccountNumber.Should().Be(source.Account.AccountNumber);
        result.Account.LegalName.Should().Be(source.Account.LegalName);
    }

    [Fact]
    public void ToDelegationRequest_WithNullNestedObjects_ShouldReturnNullNestedModels()
    {
        // Arrange
        var source = new DelegationRequestEntity
        {
            DelegationRequestId = 200,
            RequesterId = 1,
            RecipientId = 2,
            AccountId = 3,
            CreatedAt = DateTime.UtcNow,
            Status = "pending",
            RespondedAt = null,
            Requester = null!,
            Recipient = null!,
            Account = null!
        };

        // Act
        var result = source.ToDelegationRequest();

        // Assert
        result.Should().NotBeNull();
        result.Requester.Should().BeNull();
        result.Recipient.Should().BeNull();
        result.Account.Should().BeNull();
    }
}
