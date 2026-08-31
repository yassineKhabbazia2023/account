// <copyright file="AccountRoutingCodes.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Constants;

/// <summary>
/// Valeurs métier de la colonne <c>account.Account.AccountRoutingCode</c> (code de routage Akuiteo).
/// La colonne est déclarée NVARCHAR(255) à titre provisoire en attente du contrat Akuiteo définitif :
/// ces valeurs sont susceptibles d'évoluer.
/// </summary>
public static class AccountRoutingCodes
{
    /// <summary>
    /// Entité facturée en B2B. Critère d'éligibilité à la modal Sérénité.
    /// </summary>
    public const string B2B = "0-B2B";
}
