// <copyright file="MapToContactEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Mappers.EventsMapper;

public static class MapToContactEntity
{
    public static ContactEntity? ToContactEntity(this ContactStateEventData source)
    {
        return source == null ? null : new ContactEntity
        {
            ContactId = source.ContactId,
            ContactGlobalUniqueId = source.ContactGlobalUniqueId ?? Guid.Empty,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            LandPhone = source.LandPhone,
            MobilePhone = source.MobilePhone,
            Office = source.Office,
            PersonaName = source.PersonaName,
            Status = source.Status,
            Type = source.Type,
            IsActive = source.IsActive,
            CreationDate = source.CreationDate ?? DateTime.UtcNow,
        };
    }

    public static void ToContactEntity(this ContactEntity source, ContactEntity destination)
    {
        if (source == null || destination == null)
        {
            return;
        }

        destination.ContactGlobalUniqueId = source.ContactGlobalUniqueId;
        destination.FirstName = source.FirstName;
        destination.LastName = source.LastName;
        destination.Email = source.Email;
        destination.LandPhone = source.LandPhone;
        destination.MobilePhone = source.MobilePhone;
        destination.Office = source.Office;
        destination.PersonaName = source.PersonaName;
        destination.Status = source.Status;
        destination.Type = source.Type;
        destination.CreationDate = source.CreationDate;
        destination.LastUpdateDate = DateTime.UtcNow;
        destination.IsActive = source.IsActive;
    }
}
