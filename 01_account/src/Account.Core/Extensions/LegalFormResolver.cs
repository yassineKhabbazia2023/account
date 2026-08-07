// <copyright file="LegalFormResolver.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Extensions;

public static class LegalFormResolver
{
    private static readonly IReadOnlyDictionary<string, KeyValuePair<string, string>> _legalFormsByCode =
        new AccountReferentialInformation().LegalForm
            .ToDictionary(pair => pair.Key, pair => pair, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Résout la forme juridique reçue à partir du référentiel <see cref="AccountReferentialInformation.LegalForm"/>.
    /// Si la valeur correspond à un code connu (ex: "ENT IND"), retourne le code du référentiel et son libellé ("ENTREPRISE INDIVIDUELLE").
    /// Sinon, retourne la valeur d'origine comme libellé, sans code.
    /// </summary>
    /// <param name="legalForm">La forme juridique reçue (code ou libellé)</param>
    /// <returns>Le code du référentiel (ou null si inconnu) et le libellé à persister</returns>
    public static (string? Code, string? Label) Resolve(string? legalForm)
    {
        if (string.IsNullOrWhiteSpace(legalForm))
        {
            return (null, legalForm);
        }

        if (_legalFormsByCode.TryGetValue(legalForm.Trim(), out var referentialEntry))
        {
            return (referentialEntry.Key, referentialEntry.Value);
        }

        return (null, legalForm);
    }
}
