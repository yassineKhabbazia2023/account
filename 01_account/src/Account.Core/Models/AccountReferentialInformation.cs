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

    public IReadOnlyDictionary<string, string> MissionType
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "Tenue", "Tenue" },
                { "Revision", "Révision" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> LegalForm
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "ADM - ETAT", "ADMINISTRATION - ETAT" },
                { "ASS. CULTUELLE", "CONGRÉGATION OU ASSOCIATION CULTUELLE" },
                { "ASS. DE DROIT LOCAL", "ASSOCIATION DE DROIT LOCAL" },
                { "ASS. DÉCLARÉE", "ASSOCIATION DÉCLARÉE" },
                { "ASS. INS ECO", "ASSOCIATION D'INSERTION PAR L'ÉCONOMIQUE" },
                { "ASS. INTERM", "ASSOCIATION INTERMÉDIAIRE" },
                { "ASS. RUP", "ASSOCIATION RECONNUE D'UTILITÉ PUBLIQUE" },
                { "AUTO", "AUTO - AUTO ENTREPRENEUR" },
                { "AUTRE", "AUTRE" },
                { "AUTRE COOP", "AUTRE COOPÉRATIVE" },
                { "AUTRE SC", "AUTRE SOCIÉTÉ CIVILE" },
                { "CAISSE EP_PREV", "CAISSE D'EPARGNE ET DE PREVOYANCE" },
                { "COL TERR", "COLLECTIVITÉ TERRITORIALE" },
                { "COM. ENTR", "COMITÉ D' ENTREPRISE" },
                { "EARL", "EARL - ENTREPRISE AGRICOLE À RESPONSABIL" },
                { "EIRL", "EIRL" },
                { "EN COURS", "EN COURS D'IMMATRICULATION" },
                { "ENT IND", "ENTREPRISE INDIVIDUELLE" },
                { "EPA", "EPA - ETABLISSEMENT PUBLIC ADMINISTRATIF" },
                { "EPIC", "EPIC - ETABLISST PUBLIC INDUSTRIEL ET CO" },
                { "EPL", "EPL - ENTREPRISE PUBLIQUE LOCALE" },
                { "EURL", "EURL - ENTREPRISE UNIPERSONNELLE À RESPO" },
                { "FIDUCIE", "FIDUCIE" },
                { "FOND. ABRITÉE", "FONDATION ABRITÉE" },
                { "FOND. ENT", "FONDATION D'ENTREPRISE" },
                { "FOND. HOSPI", "FONDATION HOSPITALIÈRE" },
                { "FOND. PART", "FONDATION PARTENARIALE" },
                { "FOND. RUP", "FONDATION RECONNUE D'UTILITÉ PUBLIQUE" },
                { "FOND. UNIVERS", "FONDATION UNIVERSITAIRE" },
                { "FONDS COOP SCIENT", "FONDS DE COOPÉRATION SCIENTIFIQUE" },
                { "FONDS DOTAT", "FONDS DE DOTATION" },
                { "FONDS PEREN ECO", "FONDS DE PERENNITE ECONOMIQUE" },
                { "GAEC", "GAEC - GRPT EXPLOITATION AGRICOLE COMMUN" },
                { "GCS_GCSMS PRIV", "GCS & GCSMS DROIT PRIVÉ" },
                { "GCS_GCSMS PUB", "GCS & GCSMS DROIT PUBLIC" },
                { "GFA", "GFA - GRPT FONCIER AGRICOLE" },
                { "GIP", "GIP - GROUPEMENT D'INTERÊT PUBLIC" },
                { "GROUP EMPL", "GROUPEMENT D'EMPLOYEURS" },
                { "GRPT", "GRPT INTÉRÊT ÉCONOMIQUE" },
                { "IND", "INDIVISION" },
                { "OPCVM", "OPCVM" },
                { "ORDRE PRO", "ORDRE PROFESSIONNEL" },
                { "ORG MUTUALISTE", "ORGANISME MUTUALISTE" },
                { "ORG SÉCU SOC RET", "ORGANISME DE SÉCURITÉ SOCIALE, RETRAITE" },
                { "ORG SYNDICALE", "ORGANISATION SYNDICALE" },
                { "ORGCONSULAIRE", "ORGANISME CONSULAIRE" },
                { "PART", "PARTICULIER" },
                { "PMDE", "PERSONNE MORALE DE DROIT ÉTRANGER" },
                { "SA À CONS ADM", "SA À CONSEIL D'ADMINISTRATION" },
                { "SA À DIR ET CONS", "SA À DIRECTOIRE ET CONSEIL DE SURVEILLAN" },
                { "SARL", "SARL - SOCIÉTÉ À RESPONSABILITÉ LIMITÉ" },
                { "SAS", "SAS - SOCIÉTÉ PAR ACTIONS SIMPLIFIÉE" },
                { "SCA", "SCA - COMMANDITE PAR ACTIONS" },
                { "SCEA", "SCEA - STE CIVILE EXPLOITATION AGRICOLE" },
                { "SCI", "SCI - STE CIVILE IMMOBILIÈRE" },
                { "SCIC", "SOCIÉTÉ COOPÉRATIVE D'INTÉRÊT COLLECTIF" },
                { "SCOP", "SCOP - SOCIÉTÉ COOPÉRATIVE ET PARTICIPA" },
                { "SCP", "SCP - STE CIVILE PROFESSIONNELLE" },
                { "SCPI", "SCPI - SOCIETE CIVILE PLACEMENT IMMOBILI" },
                { "SCS", "SCS - COMMANDITE SIMPLE" },
                { "SE", "SE - SOCIÉTÉ EUROPÉENNE" },
                { "SEL  UNIPERS", "SEL - SOCIETE D'EXERCICE LIBÉRAL UNIPERS" },
                { "SEL À RESP LIM", "SEL À RESPONSABILITÉ LIMITÉE" },
                { "SEL COM ACT", "SEL EN COMMANDITE PAR ACTIONS" },
                { "SEL FA", "SEL À FORME ANONYME" },
                { "SEL PAR ACT SIMP", "SEL PAR ACTIONS SIMPLIFIÉES" },
                { "SFAIT", "SFAIT - SOCIÉTÉ DE FAIT" },
                { "SISA", "SISA - STE INTERPROF SOINS AMBULATOIRS" },
                { "SNC", "SNC - SOCIÉTÉ EN NOM COLLECTIF" },
                { "SOC CIV MOY", "SOCIÉTE CIVILE DE MOYENS" },
                { "SOC COOP AGR", "SOCIÉTÉ COOPÉRATIVE AGRICOLE" },
                { "SOC COOP COM", "SOCIÉTÉ COOPÉRATIVE COMMERCIALE" },
                { "SOC ETRANGERE", "SOCIETE ETRANGERE" },
                { "SPART", "SPART - SOCIÉTÉ EN PARTICIPATION" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> ContactTitle
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "M", "Monsieur" },
                { "Mme", "Madame" },
                { "Dr", "Docteur" },
                { "Pr", "Professeur" }
            };
        }
    }

    public IReadOnlyDictionary<string, string> ContactService
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "Achats", "Achats" },
                { "Autre", "Autre" },
                { "Commercial, Vente", "Commercial, Vente" },
                { "Comptabilité", "Comptabilité" },
                { "Consolidation", "Consolidation" },
                { "DF", "Direction financière" },
                { "DG", "Direction générale" },
                { "Juridique", "Juridique" },
                { "Marketing /Com", "Marketing / Communication" },
                { "R&D / Innovation", "R&D / Innovation" },
                { "RH-Formation", "Ressources Humaines Formation" },
                { "RH-Général", "Ressources Humaines Général" },
                { "RH-Paie", "Ressources Humaines Paie" },
                { "RSE", "RSE" },
                { "SI/IT", "Systèmes d'informations / IT" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> ContactHierarchicalLevel
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "Autre", "Autre" },
                { "Collaborateur", "Collaborateur" },
                { "Directeur", "Directeur" },
                { "Président / Gérant", "Président / Gérant" },
                { "Responsable", "Responsable" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> LegalStructure
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "PersonneMorale", "Personne morale" },
                { "PersonnePhysique", "Personne physique" },
            };
        }
    }

    public IReadOnlyDictionary<string, string> ContactType
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "isDigitalVaultContact", "Contact Coffre-fort numérique" },
                { "isDebtCollectionContact", "Contact recouvrement" },
                { "isMandateSignatory", "Signataire lettre de mission/mandat" }
            };
        }
    }
}
