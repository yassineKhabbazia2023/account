// <copyright file="DelegationRequestError.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

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
