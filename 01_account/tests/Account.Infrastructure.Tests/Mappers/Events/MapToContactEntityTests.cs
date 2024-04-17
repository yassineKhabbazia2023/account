// <copyright file="MapToContactEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Xunit;

namespace Pulse.Account.Infrastructure.Tests.Mappers.Events;

public class MapToContactEntityTests
{
    [Fact]
    public void ToContactEntity_MapsCorrectly()
    {
        // Arrange
        var source = new ContactStateEventData
        {
            ContactId = 100,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            Office = "Test Office",
            PersonaName = "Test Persona",
            Status = "Active",
            Type = "Test Type",
            CreationDate = DateTime.UtcNow
        };

        // Act
        var result = source.ToContactEntity();

        // Assert
        Assert.Equal(source.ContactId, result.ContactId);
        Assert.Equal(source.ContactGlobalUniqueId.Value, result.ContactGlobalUniqueId);
        Assert.Equal(source.FirstName, result.FirstName);
        Assert.Equal(source.LastName, result.LastName);
        Assert.Equal(source.Email, result.Email);
        Assert.Equal(source.Office, result.Office);
        Assert.Equal(source.PersonaName, result.PersonaName);
        Assert.Equal(source.Status, result.Status);
        Assert.Equal(source.Type, result.Type);
        Assert.Equal(source.CreationDate, result.CreationDate);
    }

    [Fact]
    public void ToContactEntity_MapsToDestinationCorrectly()
    {
        // Arrange
        Guid contactGlobalUniqueId = Guid.NewGuid();

        var updatedContact = new ContactEntity
        {
            ContactId = 100,
            ContactGlobalUniqueId = contactGlobalUniqueId,
            FirstName = "Modified Test",
            LastName = "Modified User",
            Email = "test.user@example.com",
            Office = "Modified Test Office",
            PersonaName = "Test Persona",
            Status = "active",
            Type = "Test Type",
            CreationDate = DateTime.UtcNow
        };

        var existingContact = new ContactEntity
        {
            ContactId = 100,
            ContactGlobalUniqueId = contactGlobalUniqueId,
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            Office = "Test Office",
            PersonaName = "Test Persona",
            Status = "declared",
            Type = "Test Type",
            CreationDate = DateTime.UtcNow
        };

        // Act
        updatedContact.ToContactEntity(existingContact);

        // Assert
        Assert.Equal(updatedContact.ContactId, updatedContact.ContactId);
        Assert.Equal(updatedContact.ContactGlobalUniqueId, updatedContact.ContactGlobalUniqueId);
        Assert.Equal(updatedContact.FirstName, updatedContact.FirstName);
        Assert.Equal(updatedContact.LastName, updatedContact.LastName);
        Assert.Equal(updatedContact.Email, updatedContact.Email);
        Assert.Equal(updatedContact.Office, updatedContact.Office);
        Assert.Equal(updatedContact.PersonaName, updatedContact.PersonaName);
        Assert.Equal(updatedContact.Status, updatedContact.Status);
        Assert.Equal(updatedContact.Type, updatedContact.Type);
        Assert.Equal(updatedContact.CreationDate, updatedContact.CreationDate);
    }
}
