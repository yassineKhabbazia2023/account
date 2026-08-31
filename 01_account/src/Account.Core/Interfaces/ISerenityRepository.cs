// <copyright file="ISerenityRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface ISerenityRepository
{
    /// <summary>
    /// Retourne l'état d'éligibilité Sérénité du contact : choix déjà exprimé ou non, et à défaut
    /// les entités de son portefeuille remplissant les critères portés par Account.
    /// </summary>
    /// <param name="contactId">L'identifiant du contact.</param>
    /// <returns>L'état d'éligibilité.</returns>
    Task<SerenityEligibility> GetSerenityEligibilityAsync(int contactId);

    /// <summary>
    /// Enregistre le choix du contact. Le choix est immuable : un second appel échoue.
    /// </summary>
    /// <param name="contactId">L'identifiant du contact.</param>
    /// <param name="isAccepted">Le choix exprimé.</param>
    /// <returns>Une tâche.</returns>
    Task CreateSerenityChoiceAsync(int contactId, bool isAccepted);
}
