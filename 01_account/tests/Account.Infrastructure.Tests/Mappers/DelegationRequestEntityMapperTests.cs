// <copyright file="DelegationRequestEntityMapperTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class DelegationRequestEntityMapperTests
{
    [Fact]
    public void ToEntity_ShouldMapAllFields()
    {
        // Arrange
        const int requesterId = 10;
        const int recipientId = 20;
        const int accountId = 30;
        const string status = "pending";

        // Act
        var result = DelegationRequestEntityMapper.ToEntity(requesterId, recipientId, accountId, status);

        // Assert
        result.Should().NotBeNull();
        result.RequesterId.Should().Be(requesterId);
        result.RecipientId.Should().Be(recipientId);
        result.AccountId.Should().Be(accountId);
        result.Status.Should().Be(status);
        result.RespondedAt.Should().BeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ToEntities_ShouldCreateOneEntityPerRecipientAndMapCommonFields()
    {
        // Arrange
        const int requesterId = 15;
        const int accountId = 99;
        const string status = "accepted";
        var recipientIds = new[] { 1, 2, 3 };

        // Act
        var result = DelegationRequestEntityMapper.ToEntities(requesterId, accountId, recipientIds, status);

        // Assert
        result.Should().HaveCount(recipientIds.Length);

        for (var i = 0; i < recipientIds.Length; i++)
        {
            result[i].RequesterId.Should().Be(requesterId);
            result[i].RecipientId.Should().Be(recipientIds[i]);
            result[i].AccountId.Should().Be(accountId);
            result[i].Status.Should().Be(status);
            result[i].RespondedAt.Should().BeNull();
            result[i].CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }
    }

    [Fact]
    public void ToEntities_WithEmptyRecipients_ShouldReturnEmptyList()
    {
        // Arrange
        var recipientIds = Array.Empty<int>();

        // Act
        var result = DelegationRequestEntityMapper.ToEntities(1, 2, recipientIds, "pending");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
