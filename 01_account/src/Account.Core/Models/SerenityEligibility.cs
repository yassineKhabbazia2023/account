// <copyright file="SerenityEligibility.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

/// <summary>
/// État d'éligibilité à la modal Sérénité pour un contact donné.
/// </summary>
public class SerenityEligibility
{
    /// <summary>
    /// Gets or sets a value indicating whether le contact a déjà exprimé un choix.
    /// Un choix, quelle que soit sa valeur, éteint définitivement la modal.
    /// </summary>
    public bool HasMadeChoice { get; set; }

    /// <summary>
    /// Gets or sets les identifiants des entités du portefeuille du contact qui remplissent
    /// les critères portes par Account (code de routage B2B et adresse électronique absente).
    /// Vide lorsque <see cref="HasMadeChoice"/> vaut <c>true</c>.
    /// </summary>
    public IReadOnlyCollection<int> CandidateAccountIds { get; set; } = [];
}
