// <copyright file="GetAssociatedContactsRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models.Enum;

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
        /// Gets or sets PageNumber.
        /// Numéro de page.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets Search.
        /// Nombre d'éléments par page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets ContactType.
        /// Type de contacts à retourner.
        /// </summary>
        public ContactType ContactType { get; set; } = ContactType.Customer;
    }
}
