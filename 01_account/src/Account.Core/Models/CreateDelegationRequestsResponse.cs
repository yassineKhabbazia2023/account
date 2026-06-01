// <copyright file="CreateDelegationRequestsResponse.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class CreateDelegationRequestsResponse
{
    /// <summary>
    /// Gets or sets les identifiants des destinataires pour lesquels les demandes ont été créées avec succès.
    /// </summary>
    public int[] CreatedRecipientIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Gets or sets les destinataires en erreur avec la raison associée.
    /// </summary>
    public List<RecipientError> Errors { get; set; } = new();
}

public class RecipientError
{
    /// <summary>
    /// Gets or sets l'identifiant du destinataire en erreur.
    /// </summary>
    public int RecipientId { get; set; }

    /// <summary>
    /// Gets or sets la raison de l'erreur.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
