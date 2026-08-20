// <copyright file="ElectronicAddressParser.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Extensions;

public static class ElectronicAddressParser
{
    private const char LineSeparator = '/';
    private const char FieldSeparator = '-';
    private const int FieldsPerAddress = 4;

    /// <summary>
    /// Transcode la valeur brute Akuiteo (colonne AccountElectronicAddressId : liste de sites écrasée dans une
    /// seule chaîne, une ligne par site séparée par "/", chaque ligne contenant 4 champs séparés par "-" dans
    /// l'ordre nom d'appel du site, code du site, SIRET du site, identifiant d'adressage du site) en une liste
    /// exploitable par le Front-End.
    /// </summary>
    /// <param name="rawValue">La valeur brute stockée en base (colonne AccountElectronicAddressId).</param>
    /// <returns>La liste des adresses électroniques par site, vide si la valeur brute est nulle ou vide.</returns>
    public static IReadOnlyList<ElectronicAddress> ToElectronicAddresses(this string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Array.Empty<ElectronicAddress>();
        }

        var lines = rawValue.Split(LineSeparator);
        var result = new List<ElectronicAddress>(lines.Length);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Limité à 4 segments : un éventuel "-" supplémentaire dans le dernier champ (identifiant
            // d'adressage) ne casse pas l'alignement des champs qui le précèdent.
            var fields = line.Split(FieldSeparator, FieldsPerAddress);

            result.Add(new ElectronicAddress
            {
                SiteName = GetField(fields, 0),
                SiteCode = GetField(fields, 1),
                Siret = GetField(fields, 2),
                AddressingId = GetField(fields, 3),
            });
        }

        return result;
    }

    private static string? GetField(string[] fields, int index)
    {
        return index < fields.Length ? NullIfEmpty(fields[index]) : null;
    }

    private static string? NullIfEmpty(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
