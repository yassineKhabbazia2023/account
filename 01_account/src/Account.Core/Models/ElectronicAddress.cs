// <copyright file="ElectronicAddress.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    /// <summary>
    /// Représente l'adresse électronique de facturation Akuiteo d'un site, telle qu'exploitable par le Front-End.
    /// </summary>
    public class ElectronicAddress
    {
        /// <summary>
        /// Gets or sets le nom d'appel du site.
        /// </summary>
        public string? SiteName { get; set; }

        /// <summary>
        /// Gets or sets le code du site.
        /// </summary>
        public string? SiteCode { get; set; }

        /// <summary>
        /// Gets or sets le SIRET du site.
        /// </summary>
        public string? Siret { get; set; }

        /// <summary>
        /// Gets or sets l'identifiant d'adressage électronique du site.
        /// </summary>
        public string? AddressingId { get; set; }
    }
}
