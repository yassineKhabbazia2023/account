// <copyright file="AddressingIdentification.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    /// <summary>
    /// Represents the addressing identification of a site as expected by the Front-End: a projection of the only
    /// useful fields of <see cref="ElectronicAddress"/>.
    /// </summary>
    public class AddressingIdentification
    {
        /// <summary>
        /// Gets or sets the call name of the site (<see cref="ElectronicAddress.SiteName"/>).
        /// </summary>
        public string? CallName { get; set; }

        /// <summary>
        /// Gets or sets the addressing identifier of the site (<see cref="ElectronicAddress.AddressingId"/>).
        /// </summary>
        public string? AddressingIdentifier { get; set; }
    }
}
