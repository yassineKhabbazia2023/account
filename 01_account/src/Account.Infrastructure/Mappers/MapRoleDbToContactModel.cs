// <copyright file="MapRoleDbToContactModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapRoleDbToContactModel
    {
        public static IEnumerable<Contact> MapToContacts(this ICollection<TRole> source)
        {
            return source?.Select(s => s.MapToContact() !) ?? Enumerable.Empty<Contact>();
        }

        public static Contact? MapToContact(this TRole? source)
        {
            return source == null ? null : new Contact
            {
                ContactId = source.ContactId,
                FirstName = source.Contact.FirstName,
                LastName = source.Contact.LastName,
                ContactEmail = source.Contact.ContactEmail,
            };
        }
    }
}
