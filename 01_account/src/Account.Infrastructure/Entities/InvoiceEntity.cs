// <copyright file="InvoiceEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

#nullable disable

namespace Pulse.Account.Infrastructure.Entities;

/// <summary>
/// Invoice entity.
/// </summary>
public partial class InvoiceEntity
{
    /// <summary>
    /// L'identifiant technique.
    /// </summary>
    public int InvoiceId { get; set; }

    /// <summary>
    /// Le numéro de facture (identifiant externe unique).
    /// </summary>
    public string InvoiceNumber { get; set; }

    /// <summary>
    /// Le chemin du document (nom affiché = dernier segment).
    /// </summary>
    public string DocumentPath { get; set; }

    /// <summary>
    /// Le type de facture (ex: Facture RYDGE).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// La catégorie de facture.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// La date de facturation.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// La date de dépôt.
    /// </summary>
    public DateTime DepositDate { get; set; }


    /// <summary>
    /// L'identifiant technique de l'entité.
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account navigation property.
    /// </summary>
    public virtual AccountEntity Account { get; set; }
}
