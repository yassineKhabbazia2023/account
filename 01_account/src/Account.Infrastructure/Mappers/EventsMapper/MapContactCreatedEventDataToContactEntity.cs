// <copyright file="MapContactCreatedEventDataToContactEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Mappers.EventsMapper;

public static class MapContactCreatedEventDataToContactEntity
{
    public static ContactEntity ToContactEntity(this ContactCreatedEventData source)
    {
        if (source == null)
        {
            return null!;
        }

        return new ContactEntity
        {
            ContactId = source.ContactId,
            ContactGlobalUniqueId = source.GlobalContactId,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            // Office  = source.Off
            PersonaName = source.PersonaName,
            Status = source.Status,
            Type = source.Type,
            CreationDate = source.CreationDate ?? DateTime.UtcNow,
        };
    }
}
