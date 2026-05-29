// <copyright file="DelegationEligibilityReason.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Enum;

public static class DelegationEligibilityReason
{
    /// <summary>
    /// L'utilisateur a déjà un accès au dossier (via rôle existant ou demande)
    /// </summary>
    public const string AlreadyInPortfolio = "AlreadyInPortfolio";

    /// <summary>
    /// Une demande de délégation est déjà en attente pour ce dossier
    /// </summary>
    public const string AlreadyPending = "AlreadyPending";
}
