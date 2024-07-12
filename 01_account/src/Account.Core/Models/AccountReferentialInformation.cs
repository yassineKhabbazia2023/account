// <copyright file="AccountReferentialInformation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class AccountReferentialInformation
{
    public IReadOnlyDictionary<string, string> StaffSizeRange
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "=0", "0 salarié" },
                { "=1", "1 salarié" },
                { "<11", "De 2 à 10 salariés" },
                { "<50", "De 11 à 49 salariés" },
                { "<300", "De 50 à 299 salariés" },
                { ">=300", "300 salariés et plus" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> FiscalExerciseDuration
    {
        get
        {
            return Enumerable.Range(1, 24)
                .ToDictionary(
                    key => key.ToString(),
                    value => value.ToString());
        }
    }

    public IReadOnlyDictionary<string, string> AccountingType
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "Engagement", "Engagement" },
                { "Tresorerie", "Trésorerie" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> ActivityType
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "PS", "Prestation de services" },
                { "VB", "Vente de biens" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> FiscalSystem
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "BIC", "BIC (Bénéfices Industriels et Commerciaux)" },
                { "BNC", "BNC (Bénéfices Non Commerciaux)" },
                { "ME", "Micro-entreprise" },
                { "RF", "Revenus fonciers" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> VatType
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "Encaissement", "Encaissement" },
                { "Debit", "Débit" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> VatSystem
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "CA3M", "Réel normal - CA3 mensuelle" },
                { "CA3T", "Réel normal - CA3 trimestrielle" },
                { "CA12", "Réel simplifié - CA12 annuelle" },
                { "EXO", "Franchise - Exonération de TVA" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> TaxationSystem
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "IR", "Impôt sur le revenu" },
                { "IS", "Impôt sur les sociétés" },
            };
        }
    }
}
