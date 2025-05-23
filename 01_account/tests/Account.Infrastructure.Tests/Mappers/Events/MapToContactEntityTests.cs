// <copyright file="MapToContactEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Back.Events.IntegrationEvents.EventsData;

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
            LandPhone = "0102030405",
            MobilePhone = "0607080900",
            Office = "Test Office",
            PersonaName = "Test Persona",
            Status = "Active",
            Type = "Test Type",
            CreationDate = DateTime.UtcNow
        };

        // Act
        var result = source.ToContactEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(source.ContactId, result.ContactId);
        Assert.Equal(source.ContactGlobalUniqueId.Value, result.ContactGlobalUniqueId);
        Assert.Equal(source.FirstName, result.FirstName);
        Assert.Equal(source.LastName, result.LastName);
        Assert.Equal(source.Email, result.Email);
        Assert.Equal(source.LandPhone, result.LandPhone);
        Assert.Equal(source.MobilePhone, result.MobilePhone);
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
            LandPhone = "0102030405",
            MobilePhone = "0607080900",
            Office = "Test Office",
            PersonaName = "Test Persona",
            Status = "declared",
            Type = "Test Type",
            CreationDate = DateTime.UtcNow
        };

        // Act
        existingContact.ToContactEntity(updatedContact);

        // Assert
        Assert.Equal(existingContact.ContactId, updatedContact.ContactId);
        Assert.Equal(existingContact.ContactGlobalUniqueId, updatedContact.ContactGlobalUniqueId);
        Assert.Equal(existingContact.FirstName, updatedContact.FirstName);
        Assert.Equal(existingContact.LastName, updatedContact.LastName);
        Assert.Equal(existingContact.Email, updatedContact.Email);
        Assert.Equal(existingContact.LandPhone, updatedContact.LandPhone);
        Assert.Equal(existingContact.MobilePhone, updatedContact.MobilePhone);
        Assert.Equal(existingContact.Office, updatedContact.Office);
        Assert.Equal(existingContact.PersonaName, updatedContact.PersonaName);
        Assert.Equal(existingContact.Status, updatedContact.Status);
        Assert.Equal(existingContact.Type, updatedContact.Type);
        Assert.Equal(existingContact.CreationDate, updatedContact.CreationDate);
        Assert.NotNull(updatedContact.LastUpdateDate);
        Assert.NotEqual(existingContact.LastUpdateDate, updatedContact.LastUpdateDate);
    }

    [Fact]
    public void ToContactEntity_ReturnNull_IfContactStateEventDataIsNull()
    {
        // arrange
        ContactStateEventData source = null;

        // act 
        var result = source.ToContactEntity();

        // arrange
        result.Should().BeNull();
    }

    [Fact]
    public void ToContactEntity_BothSourceAndDestinationNull_ShouldNotThrowException()
    {
        // Arrange
        ContactEntity source = null;
        ContactEntity destination = null;

        // Act & Assert
        var exception = Record.Exception(() => source.ToContactEntity(destination));
        Assert.Null(exception);
    }

    [Fact]
    public void ToContactEntity_SourceNull_ShouldNotThrowException()
    {
        // Arrange
        ContactEntity source = null;
        var destination = new ContactEntity();

        // Act & Assert
        var exception = Record.Exception(() => source.ToContactEntity(destination));
        Assert.Null(exception);
    }

    [Fact]
    public void ToContactEntity_DestinationNull_ShouldNotThrowException()
    {
        // Arrange
        var source = new ContactEntity();
        ContactEntity destination = null;

        // Act & Assert
        var exception = Record.Exception(() => source.ToContactEntity(destination));
        Assert.Null(exception);
    }
}
