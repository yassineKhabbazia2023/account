// <copyright file="InvoiceEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

#nullable disable
using System;

namespace Pulse.Account.Infrastructure.Entities;

public partial class InvoiceEntity
{
    /// <summary>
    /// L'identifiant technique
    /// </summary>
    public int InvoiceId { get; set; }

    /// <summary>
    /// Le numéro de facture (identifiant externe unique)
    /// </summary>
    public string InvoiceNumber { get; set; }

    /// <summary>
    /// Le nom de la facture
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// La date de facturation
    /// </summary>
    public DateTime? InvoiceDate { get; set; }

    /// <summary>
    /// La date de dépôt
    /// </summary>
    public DateTime? DepositDate { get; set; }

    /// <summary>
    /// L'identifiant technique de l'entité
    /// </summary>
    public int AccountId { get; set; }


    public virtual AccountEntity Account { get; set; }
}
