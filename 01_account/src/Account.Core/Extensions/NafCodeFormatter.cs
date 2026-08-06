// <copyright file="NafCodeFormatter.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Extensions;

public static class NafCodeFormatter
{
    /// <summary>
    /// Formate le code NAF au format standard INSEE : XX.XXZ (5 caractères avec point à la position 3).
    /// Ex: "6234Z" devient "62.34Z", "62.34Z" reste "62.34Z".
    /// </summary>
    /// <param name="nafCode">Le code NAF à formater</param>
    /// <returns>Le code NAF formaté, ou la valeur d'origine si le format est incorrect</returns>
    public static string? Format(string? nafCode)
    {
        if (string.IsNullOrWhiteSpace(nafCode))
        {
            return nafCode;
        }

        // Supprimer les points existants et les espaces
        var cleanedCode = nafCode.Replace(".", "").Trim();

        // Vérifier si le code a exactement 5 caractères
        if (cleanedCode.Length != 5)
        {
            return nafCode; // Retourner l'original si format incorrect
        }

        // Ajouter le point à la 3ème position : XX.XXZ
        return $"{cleanedCode.Substring(0, 2)}.{cleanedCode.Substring(2)}";
    }
}
