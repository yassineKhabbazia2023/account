// <copyright file="GetAssociatedContactsRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;

namespace Pulse.Account.Core.Requests
{
    public class GetAssociatedContactsRequest
    {
        /// <summary>
        /// Gets or sets Search.
        /// Critère de recherche.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Gets or sets ContactType.
        /// Type de contacts à retourner.
        /// </summary>
        public ContactType ContactType { get; set; } = ContactType.Customer;

        public Sorting? Sorting { get; set; }
    }
}
