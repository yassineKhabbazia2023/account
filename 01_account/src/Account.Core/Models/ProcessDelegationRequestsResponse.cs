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

    /// <summary>
    /// Gets or sets les identifiants des demandeurs dont toutes les demandes ont été refusées.
    /// Utilisé pour déclencher la notification de refus côté front/événement.
    /// </summary>
    public int[] AllRefusedRequesterIds { get; set; } = Array.Empty<int>();
}