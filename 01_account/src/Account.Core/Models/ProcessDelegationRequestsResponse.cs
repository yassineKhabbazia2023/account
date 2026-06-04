// <copyright file="ProcessDelegationRequestsResponse.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class ProcessDelegationRequestsResponse
{
    /// <summary>
    /// Gets or sets les identifiants des demandes traitées avec succès.
    /// </summary>
    public int[] ProcessedIds { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Gets or sets les demandes en erreur avec la raison associée.
    /// </summary>
    public List<DelegationRequestError> Errors { get; set; } = new();
}

public class DelegationRequestError
{
    /// <summary>
    /// Gets or sets l'identifiant de la demande en erreur.
    /// </summary>
    public int DelegationRequestId { get; set; }

    /// <summary>
    /// Gets or sets la raison de l'erreur.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}